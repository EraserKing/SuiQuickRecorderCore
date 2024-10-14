using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SuiQuickRecorderCore.Models.Records
{
    public sealed class SuiRecordNetworkRequest
    {
        public string Endpoint { get; private set; }

        public List<KeyValuePair<string, string>> Content { get; private set; }

        public Task<HttpResponseMessage> ResponseMessage { get; set; } = null;

        public SuiRecordNetworkRequest(string endpoint, List<KeyValuePair<string, string>> content)
        {
            Endpoint = endpoint;
            Content = content;
        }

        public bool IsSuccess { get => ResponseMessage != null && ResponseMessage.IsCompleted && ResponseMessage.Result.IsSuccessStatusCode && !ResponseMessage.Result.Content.ReadAsStringAsync().Result.Contains("\"resCode\":500"); }

        public void SendRequest(HttpClient client, string baseUrl)
        {
            var formStr = string.Join('&', Content.Select(kv => $"{kv.Key}={HttpUtility.UrlEncode(kv.Value)}"));
            var formCtx = new StringContent(formStr, Encoding.UTF8, "application/x-www-form-urlencoded");

            ResponseMessage = client.PostAsync($"{baseUrl}/{Endpoint}", formCtx);
        }
    }
}
