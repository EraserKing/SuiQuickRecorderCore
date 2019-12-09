using CsvHelper;
using SuiQuickRecorderCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SuiQuickRecorderCore.Controllers
{
    public class SuiQuickRecorderController
    {
        private HttpClient Client { get; set; }
        private SuiQuickRecorderControllerOptions Options { get; set; }

        public SuiKVPairs Accounts { get; private set; }
        public SuiKVPairs CategoriesIn { get; private set; }
        public SuiKVPairs CategoriesOut { get; private set; }
        public SuiKVPairs Stores { get; private set; }

        private IEnumerable<ISuiRecord> Records { get; set; }

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

            Client = new HttpClient(clientHandler);

            Client.DefaultRequestHeaders.Add("accept", "*/*");
            Client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
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

            Accounts = new SuiKVPairs(Options.AccountsFile);
            CategoriesIn = new SuiKVPairs(Options.CategoriesInFile);
            CategoriesOut = new SuiKVPairs(Options.CategoriesOutFile);
            Stores = new SuiKVPairs(Options.StoresFile);
        }

        public void LoadRecords(string recordFile)
        {
            TextReader reader = new StreamReader(recordFile);
            CsvReader csvReader = new CsvReader(reader);

            // Force immediately read - otherwise a delayed read would happen on a closed stream
            Records = csvReader.GetRecords<SuiRecordOrigin>().Select(x => SuiRecordFactory.Create(x, Accounts, CategoriesIn, CategoriesOut, Stores)).ToArray();

            reader.Close();
        }

        public string SendLoadedRecords()
        {
            var tasks = Records.Select(x => SendRecord(x)).ToArray();
            Task.WaitAll(tasks);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{tasks.Count(x => x.Result.IsSuccessStatusCode)} / {tasks.Length} task(s) submitted");
            sb.Append(string.Join(Environment.NewLine, tasks.Select(x => x.Result.Content.ReadAsStringAsync().Result)));

            return sb.ToString();
        }

        private Task<HttpResponseMessage> SendRecord(ISuiRecord record)
        {
            return Client.PostAsync($"{Options.ApiBaseUrl}/{record.GetNetworkRequestEndpoint()}", new FormUrlEncodedContent(record.ToNetworkRequestBody()));
        }
    }
}
