using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuiQuickRecorderCore.Models.KeyValue;
using SuiQuickRecorderCore.Models.Options;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderCore.Services;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SuiQuickRecorderCLI
{
    internal class Program
    {
        private static SuiKVPairs LoadKv(string path)
        {
            var kvPairs = new SuiKVPairs();
            using var reader = new StreamReader(path);
            var kvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            };
            using var csvReader = new CsvReader(reader, kvConfig);

            foreach (var pair in csvReader.GetRecords<SuiKVRecord>())
            {
                kvPairs.Add(pair.Name, pair.Id);
                foreach (var altValue in pair.Alts.Where(x => !string.IsNullOrEmpty(x)))
                {
                    kvPairs.Add(altValue, pair.Id);
                }
            }
            return kvPairs;
        }

        private static async Task Main(string[] args)
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<SuiQuickRecorderService>();
            var sessionLogger = loggerFactory.CreateLogger<SuiSessionService>();

            var options = new SuiQuickRecorderServiceOptions();
            var reference = new SuiRecordReference(
                LoadKv("dataAccounts.csv"),
                LoadKv("dataCategoriesIn.csv"),
                LoadKv("dataCategoriesOut.csv"),
                LoadKv("dataStores.csv"),
                LoadKv("loaners.csv")
            );

            // Create SessionService which manages HttpClient and CookieContainer
            using var sessionService = new SuiSessionService(Options.Create(options), sessionLogger);

            SuiQuickRecorderService service = new SuiQuickRecorderService(
                sessionService,
                logger
            );

            // Read Credentials
            var credLines = await File.ReadAllLinesAsync("credentials.txt");
            if (credLines.Length < 2) throw new Exception("Invalid creds");
            await sessionService.LoginAsync(credLines[0], credLines[1]);

            // Load Records
            var recordReader = new StreamReader("records.csv");
            var recordConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            };
            var csvReader = new CsvReader(recordReader, recordConfig);
            var origins = csvReader.GetRecords<SuiRecordOrigin>().ToList(); // Materialize DTOs

            // Process Stage
            var processResult = service.ProcessRecords(origins, reference);

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("Process Result:");
            foreach (var line in processResult.Lines)
            {
                if (!line.Success)
                    foreach (var e in line.Errors)
                        Console.WriteLine($"  [Line {line.Line}] ERROR: {e}");
            }

            if (processResult.Lines.Any(l => !l.Success))
            {
                Console.WriteLine("Process failed. Aborting send.");
                return;
            }

            // Send Stage
            var result = await service.SendRecordsAsync(origins, reference);

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Send Completed.");
            Console.WriteLine($"Records: {result.SuccessRecords}/{result.TotalRecords}");
            Console.WriteLine($"Requests: {result.SuccessRequests}/{result.TotalRequests}");

            if (result.Lines.Any())
            {
                Console.WriteLine("Errors:");
                foreach (var line in result.Lines)
                    foreach (var e in line.Errors)
                        Console.WriteLine($"  [Line {line.Line}] {e}");
            }
        }
    }
}