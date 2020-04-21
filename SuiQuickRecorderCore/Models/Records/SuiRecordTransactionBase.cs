using SuiQuickRecorderCore.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace SuiQuickRecorderCore.Models.Records
{
    public abstract class SuiRecordTransactionBase : ISuiRecord
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

        public SuiRecordType RecordType { get; set; }

        protected SuiRecordTransactionBase(string date, string price, string memo, SuiRecordType recordType)
        {
            DateTime parsedDate = DateTime.Parse(DateTime.Now.Year + "-" + date.Substring(0, 2) + "-" + date.Substring(2, 2));
            if ((parsedDate - DateTime.Now).Days > 30) // If the record is too new (newer than today + 30 days), the record should go to the last year
            {
                parsedDate = parsedDate.AddYears(-1);
            }

            Time = $"{parsedDate:yyyy-MM-dd} 10:00";
            Price = price;
            Memo = memo;

            RecordType = recordType;
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

        public virtual string GetNetworkRequestEndpoint() => throw new Exception("NO ENDPOINT DEFINED");

        public virtual IEnumerable<SuiRecordNetworkRequest> CreateNetworkRequests()
        {
            return new List<SuiRecordNetworkRequest>() {
            new SuiRecordNetworkRequest(
                GetNetworkRequestEndpoint(),
                ToNetworkRequestBody()
                )};
        }
    }
}
