using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuiQuickRecorderCore.Models.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SuiQuickRecorderCore.Services
{
    public class SuiSessionService : IDisposable
    {
        public HttpClient Client { get; private set; }
        private readonly IOptions<SuiQuickRecorderServiceOptions> _options;
        private readonly ILogger<SuiSessionService> _logger;
        private CancellationTokenSource _sessionCts;

        public bool IsLoggedIn { get; private set; } = false;

        public SuiSessionService(IOptions<SuiQuickRecorderServiceOptions> options, ILogger<SuiSessionService> logger)
        {
            _options = options;
            _logger = logger;
            InitializeClient();
        }

        private void InitializeClient()
        {
            if (Client != null) return;

            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.All,
                UseCookies = true
            };

            if (!string.IsNullOrEmpty(_options.Value.ProxyHost))
            {
                handler.Proxy = new WebProxy(_options.Value.ProxyHost, _options.Value.ProxyPort ?? 80);
                handler.UseProxy = true;
            }

            Client = new HttpClient(handler);

            var baseUrl = _options.Value.ApiBaseUrl;
            if (string.IsNullOrEmpty(baseUrl)) baseUrl = "https://www.sui.com";

            Client.BaseAddress = new Uri(baseUrl);

            // Headers
            Client.DefaultRequestHeaders.TryAddWithoutValidation("accept", "*/*");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("accept-language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("cache-control", "no-cache");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("dnt", "1");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("origin", baseUrl);
            Client.DefaultRequestHeaders.TryAddWithoutValidation("pragma", "no-cache");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("referer", $"{baseUrl}/tally/new.do");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-mode", "cors");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("x-requested-with", "XMLHttpRequest");
        }

        public async Task LoginAsync(string username, string password)
        {
            _sessionCts?.Cancel();

            if (Client == null) InitializeClient();

            // Step 1
            var response1 = await Client.GetAsync("https://login.sui.com/login.jsp?returnUrl=https%3A%2F%2Fwww.sui.com%2Fsso%2Fredirect.do");
            response1.EnsureSuccessStatusCode();
            _logger.LogDebug("Accessed login page.");

            // Step 2
            var response2 = await Client.GetAsync("https://login.sui.com/login.do?opt=vccode");
            var json2 = await response2.Content.ReadAsStringAsync();

            var uidMatch = Regex.Match(json2, "\"uid\"\\s*:\\s*\"([^\"]+)\"");
            var vccodeMatch = Regex.Match(json2, "\"vccode\"\\s*:\\s*\"([^\"]+)\"");

            if (!uidMatch.Success || !vccodeMatch.Success)
            {
                throw new InvalidOperationException("Failed to get uid/vccode");
            }

            var uid = uidMatch.Groups[1].Value;
            var vccode = vccodeMatch.Groups[1].Value;
            _logger.LogDebug("Obtained uid and vccode.");

            var h1 = CalculateSha1(password);
            var h2 = CalculateSha1(username + h1);
            var h3 = CalculateSha1(h2 + vccode);

            _logger.LogDebug("Calculated hashed password.");

            // Step 3
            var step3Url = $"https://login.sui.com/login.do?email={Uri.EscapeDataString(username)}&status=1&password={h3}&uid={uid}";
            var response3 = await Client.GetAsync(step3Url);
            response3.EnsureSuccessStatusCode();
            _logger.LogInformation("Sent login request.");

            // Step 4
            var response4 = await Client.PostAsync("https://login.sui.com/auth.do?returnUrl=https://www.sui.com/sso/redirect.do", null);
            var html4 = await response4.Content.ReadAsStringAsync();

            var su = GetInputValue(html4, "su");
            var iv = GetInputValue(html4, "iv");
            var sign = GetInputValue(html4, "sign");
            var st = GetInputValue(html4, "st");

            if (su == null || iv == null || sign == null || st == null)
            {
                throw new InvalidOperationException("Failed to get auth values");
            }

            _logger.LogDebug("Obtained su / iv / sign / st values.");

            // Step 5
            var response5 = await Client.PostAsync("https://www.sui.com/sso/redirect.do", new FormUrlEncodedContent(new Dictionary<string, string> {
                {"su", su}, {"iv", iv}, {"sign", sign}, {"st", st}
            }));
            var html5 = await response5.Content.ReadAsStringAsync();

            var st2 = GetInputValue(html5, "st");

            if (st2 == null)
            {
                throw new InvalidOperationException("Failed to get final st value");
            }
            _logger.LogDebug("Obtained final st value.");

            // Step 6
            var response6 = await Client.PostAsync("https://www.sui.com/report_index.do", new FormUrlEncodedContent(new Dictionary<string, string> {
                {"st", st2}
            }));
            response6.EnsureSuccessStatusCode();
            IsLoggedIn = true;
            _logger.LogInformation("Logged in successfully.");

            // Auto-dispose after 5 minutes
            _sessionCts = new CancellationTokenSource();
            var token = _sessionCts.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), token);
                    if (!token.IsCancellationRequested)
                    {
                        DisposeClient();
                        _logger.LogInformation("Session timed out. Client disposed.");
                    }
                }
                catch (TaskCanceledException) { }
            }, token);
        }

        private void DisposeClient()
        {
            IsLoggedIn = false;
            if (Client != null)
            {
                Client.Dispose();
                Client = null;
            }
        }

        public void Dispose()
        {
            _sessionCts?.Cancel();
            DisposeClient();
        }

        private string CalculateSha1(string input)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private string GetInputValue(string html, string name)
        {
            var match = Regex.Match(html, $"<input type=hidden name=\"{name}\" value=\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : null;
        }

        public async Task<bool> IsCredentialValidAsync()
        {
            var result = await Client.GetAsync("report_index.do");
            var content = await result.Content.ReadAsStringAsync();
            return content.Contains("个人中心");
        }
    }
}