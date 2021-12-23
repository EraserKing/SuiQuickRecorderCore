using CsvHelper;
using SuiQuickRecorderCore.Models;
using SuiQuickRecorderCore.Models.Interfaces;
using SuiQuickRecorderCore.Models.KeyValue;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderCore.Models.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace SuiQuickRecorderCore.Controllers
{
    public class SuiQuickRecorderController
    {
        private HttpClient Client { get; set; }
        private SuiQuickRecorderControllerOptions Options { get; set; }
        public SuiRecordReference Reference { get; set; }

        private List<ISuiRecord> Records { get; set; }

        public static readonly Regex MemoVariable = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);

        public SuiQuickRecorderController(SuiQuickRecorderControllerOptions options)
        {
            Options = options;

            HttpClientHandler clientHandler = new HttpClientHandler();

            clientHandler.CookieContainer = new CookieContainer();
            foreach (var line in File.ReadAllText(Options.CookiesFile).Split(';'))
            {
                clientHandler.CookieContainer.Add(new Uri(Options.ApiBaseUrl), new Cookie(line.Substring(0, line.IndexOf('=')).Trim(), line.Substring(line.IndexOf('=') + 1).Trim()));
            }

            if (!string.IsNullOrEmpty(Options.ProxyHost) && Options.ProxyPort.HasValue)
            {
                clientHandler.Proxy = new WebProxy(Options.ProxyHost, Options.ProxyPort.Value);
            }

            clientHandler.AutomaticDecompression = DecompressionMethods.All;

            Client = new HttpClient(clientHandler);

            Client.DefaultRequestHeaders.Add("accept", "*/*");
            Client.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            Client.DefaultRequestHeaders.Add("cache-control", "no-cache");
            Client.DefaultRequestHeaders.Add("dnt", "1");
            Client.DefaultRequestHeaders.Add("origin", Options.ApiBaseUrl);
            Client.DefaultRequestHeaders.Add("pragma", "no-cache");
            Client.DefaultRequestHeaders.Add("referer", $"{Options.ApiBaseUrl}/tally/new.do");
            Client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            Client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
            Client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");

            Reference = new SuiRecordReference(
                new SuiKVPairs(Options.AccountsFile),
                new SuiKVPairs(Options.CategoriesInFile),
                new SuiKVPairs(Options.CategoriesOutFile),
                new SuiKVPairs(Options.StoresFile),
                new SuiKVPairs(Options.LoanersFile));
        }

        public void LoadRecords(string recordFile = "records.csv")
        {
            TextReader reader = new StreamReader(new FileStream(recordFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            CsvReader csvReader = new CsvReader(reader);

            List<ArgumentOutOfRangeException> createExceptions = new List<ArgumentOutOfRangeException>();

            // Force immediately read - otherwise a delayed read would happen on a closed stream
            Records = csvReader.GetRecords<SuiRecordOrigin>().Select((x, l) =>
            {
                try
                {
                    return SuiRecordFactory.Create(x, Reference);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    createExceptions.Add(new ArgumentOutOfRangeException($"An error occurred, *maybe* on line {l + 1}", ex));
                    return null;
                }
            }).ToList();

            if (createExceptions.Count > 0)
            {
                throw new AggregateException($"Some error(s) occurred when reading records: {Environment.NewLine}{string.Join(Environment.NewLine, createExceptions.Select(x => $"{x.Message}: {x.InnerException.Message}"))}", createExceptions);
            }

            foreach (var record in Records)
            {
                var matches = MemoVariable.Matches(record.Memo);
                if (matches.Count > 0)
                {
                    string originalMemo = record.Memo;
                    foreach (Match match in matches)
                    {
                        string variableName = match.Groups[1].Value;
                        switch (variableName)
                        {
                            case "Id":
                                originalMemo.Replace(match.Groups[0].Value, record.Id);
                                break;

                            case "Store":
                                originalMemo.Replace(match.Groups[0].Value, record.Store);
                                break;

                            case "Time":
                                originalMemo.Replace(match.Groups[0].Value, record.Time);
                                break;

                            case "Project":
                                originalMemo.Replace(match.Groups[0].Value, record.Project);
                                break;

                            case "Member":
                                originalMemo.Replace(match.Groups[0].Value, record.Member);
                                break;

                            /* Memo cannot be replaced in case of dead loop
                            case "Memo":
                                originalMemo.Replace(match.Groups[0].Value, record.Memo);
                                break; */

                            case "Url":
                                originalMemo.Replace(match.Groups[0].Value, record.Url);
                                break;

                            case "OutAccount":
                                originalMemo.Replace(match.Groups[0].Value, record.OutAccount);
                                break;

                            case "InAccount":
                                originalMemo.Replace(match.Groups[0].Value, record.InAccount);
                                break;

                            case "DebtAccount":
                                originalMemo.Replace(match.Groups[0].Value, record.DebtAccount);
                                break;

                            case "Account":
                                originalMemo.Replace(match.Groups[0].Value, record.Account);
                                break;

                            case "Price":
                                originalMemo.Replace(match.Groups[0].Value, record.Price);
                                break;

                            case "OriginalDate":
                                originalMemo.Replace(match.Groups[0].Value, record.OriginalDate);
                                break;
                        }
                    }

                    record.Memo = originalMemo;
                }

                record.Price = record.Price.Split('+').Select(x => decimal.Parse(x)).Sum().ToString();
            }

            // Remove loan records, combine and re-add
            var loanRecords = SuiRecordLoan.AutoCombine(Records.Where(x => x.RecordType == SuiRecordType.Loan).Cast<SuiRecordLoan>()).ToArray();
            Records.RemoveAll(x => x.RecordType == SuiRecordType.Loan);
            Records.AddRange(loanRecords);

            reader.Close();
        }

        public IEnumerable<string> SendLoadedRecords()
        {
            int successRecords = 0;
            int totalRecords = 0;

            int successRequests = 0;
            int totalRequests = 0;

            foreach (var record in Records)
            {
                totalRecords++;
                bool recordSuccess = true;

                foreach (var networkRequest in record.CreateNetworkRequests())
                {
                    totalRequests++;
                    int retryCount = 0;

                    do
                    {
                        if (retryCount > 0)
                        {
                            yield return $"RETRY ATTEMPT #{retryCount}";
                        }
                        networkRequest.SendRequest(Client, Options.ApiBaseUrl);
                        yield return networkRequest.ResponseMessage.Result.Content.ReadAsStringAsync().Result;
                    }
                    while (!networkRequest.IsSuccess && retryCount++ < 10);
                    if (retryCount >= 10)
                    {
                        yield return "ALL RETRY ATTEMPT FAILED";
                        recordSuccess = false;
                    }

                    if (networkRequest.IsSuccess)
                    {
                        successRequests++;
                    }
                }

                if (recordSuccess)
                {
                    successRecords++;
                }
            }
            yield return $"Records success {successRecords} / total {totalRecords}, requests success {successRequests} / total {totalRequests}";
        }

        public bool IsCredentialValid()
        {
            var result = Client.GetAsync($"{Options.ApiBaseUrl}/report_index.do").Result;
            return result.StatusCode != HttpStatusCode.Found; // Redirect to log in page or not?
        }
    }
}
