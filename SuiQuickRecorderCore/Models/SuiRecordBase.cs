using System;
using System.Collections.Generic;
using System.Text;

namespace SuiQuickRecorderCore.Models
{
    public interface ISuiRecord
    {
        string Id { get; set; }
        string Store { get; set; }
        string Time { get; set; }
        string Project { get; set; }
        string Member { get; set; }
        string Memo { get; set; }
        string Url { get; set; }
        string OutAccount { get; set; }
        string InAccount { get; set; }
        string DebtAccount { get; set; }
        string Account { get; set; }
        string Price { get; set; }

        List<KeyValuePair<string, string>> ToNetworkRequestBody();

        string GetNetworkRequestEndpoint();
    }

    public abstract class SuiRecordBase : ISuiRecord
    {
        public string Id { get; set; } = "0";
        public string Store { get; set; } = "0";
        public string Time { get; set; } = "";
        public string Project { get; set; } = "0";
        public string Member { get; set; } = "0";
        public string Memo { get; set; } = "";
        public string Url { get; set; } = "";
        public string OutAccount { get; set; } = "";
        public string InAccount { get; set; } = "";
        public string DebtAccount { get; set; } = "";
        public string Account { get; set; } = "0";
        public string Price { get; set; } = "";

        protected SuiRecordBase(string date, string price, string memo)
        {
            Time = $"{DateTime.Parse(DateTime.Now.Year + "-" + date.Substring(0, 2) + "-" + date.Substring(2, 2)).ToString("yyyy-MM-dd")} 10:00";
            Price = price;
            Memo = memo;
        }

        public virtual List<KeyValuePair<string, string>> ToNetworkRequestBody()
        {
            List<KeyValuePair<string, string>> record = new List<KeyValuePair<string, string>>();
            record.Add(new KeyValuePair<string, string>("id", Id));
            record.Add(new KeyValuePair<string, string>("store", Store));
            record.Add(new KeyValuePair<string, string>("time", Time));
            record.Add(new KeyValuePair<string, string>("project", Project));
            record.Add(new KeyValuePair<string, string>("member", Member));
            record.Add(new KeyValuePair<string, string>("memo", Memo));
            record.Add(new KeyValuePair<string, string>("url", Url));
            record.Add(new KeyValuePair<string, string>("out_account", OutAccount));
            record.Add(new KeyValuePair<string, string>("in_account", InAccount));
            record.Add(new KeyValuePair<string, string>("debt_account", DebtAccount));
            record.Add(new KeyValuePair<string, string>("account", Account));
            record.Add(new KeyValuePair<string, string>("price", Price));

            return record;
        }

        public abstract string GetNetworkRequestEndpoint();
    }
}
