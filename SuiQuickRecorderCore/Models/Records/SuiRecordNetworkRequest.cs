using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
            ResponseMessage = client.PostAsync($"{baseUrl}/{Endpoint}", new FormUrlEncodedContent(Content));
        }
    }
}
