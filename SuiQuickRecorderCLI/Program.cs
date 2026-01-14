using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SuiQuickRecorderCore.Models.KeyValue;
using SuiQuickRecorderCore.Models.Options;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderCore.Services;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SuiQuickRecorderCLI
{
    internal class Program
    {
        private static SuiKVPairs LoadKv(string path)
        {
            var kvPairs = new SuiKVPairs();
            using var reader = new StreamReader(path);
            using var csvReader = new CsvReader(reader);
            csvReader.Configuration.HeaderValidated = null;
            csvReader.Configuration.MissingFieldFound = null;

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
            var csvReader = new CsvReader(recordReader);
            csvReader.Configuration.HeaderValidated = null;
            csvReader.Configuration.MissingFieldFound = null;
            var origins = csvReader.GetRecords<SuiRecordOrigin>().ToList(); // Materialize DTOs

            var result = await service.SendRecordsAsync(origins, reference);
            
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Processing Completed.");
            Console.WriteLine($"Records: {result.SuccessRecords}/{result.TotalRecords}");
            Console.WriteLine($"Requests: {result.SuccessRequests}/{result.TotalRequests}");
            
            if (result.Warnings.Any())
            {
                Console.WriteLine("Warnings:");
                foreach (var w in result.Warnings) Console.WriteLine($" - {w}");
            }
             if (result.Errors.Any())
            {
                Console.WriteLine("Errors:");
                foreach (var e in result.Errors) Console.WriteLine($" - {e}");
            }
        }
    }
}