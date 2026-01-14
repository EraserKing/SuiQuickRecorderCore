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
        
        public string ResponseContent { get; set; }

        public SuiRecordNetworkRequest(string endpoint, List<KeyValuePair<string, string>> content)
        {
            Endpoint = endpoint;
            Content = content;
        }

        public bool IsSuccess(HttpResponseMessage response, string content)
        {
            if (!response.IsSuccessStatusCode) return false;
            return !content.Contains("\"resCode\":500");
        }
    }
}
