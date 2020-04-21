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

        public Task<HttpResponseMessage> ResponseMessage { get; set; }

        public SuiRecordNetworkRequest(string endpoint, List<KeyValuePair<string, string>> content)
        {
            Endpoint = endpoint;
            Content = content;
        }
    }
}
