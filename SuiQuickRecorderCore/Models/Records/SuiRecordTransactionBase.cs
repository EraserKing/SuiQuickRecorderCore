using SuiQuickRecorderCore.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        public string OriginalDate { get; set; }

        public SuiRecordType RecordType { get; set; }

        public static readonly Regex MemoVariable = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);

        protected SuiRecordTransactionBase(string date, string price, string memo, SuiRecordType recordType)
        {
            OriginalDate = date;

            DateTime parsedDate = DateTime.Parse(DateTime.Now.Year + "-" + date.Substring(0, 2) + "-" + date.Substring(2, 2));
            if ((parsedDate - DateTime.Now).Days > 30) // If the record is too new (newer than today + 30 days), the record should go to the last year
            {
                parsedDate = parsedDate.AddYears(-1);
            }

            Time = $"{parsedDate:yyyy-MM-dd} 10:00";
            Price = string.Join(",", price.Split(",").Select(x => x.Split('+').Select(x => decimal.Parse(x)).Sum().ToString()));
            Memo = memo;

            RecordType = recordType;
        }

        protected void UpdateMemo()
        {
            var matches = MemoVariable.Matches(Memo);
            if (matches.Count > 0)
            {
                string originalMemo = Memo;
                foreach (Match match in matches)
                {
                    string variableName = match.Groups[1].Value;
                    switch (variableName)
                    {
                        case "Id":
                            originalMemo= originalMemo.Replace(match.Groups[0].Value, Id);
                            break;

                        case "Store":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, Store);
                            break;

                        case "Time":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, Time);
                            break;

                        case "Project":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, Project);
                            break;

                        case "Member":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, Member);
                            break;

                        /* Memo cannot be replaced in case of dead loop
                        case "Memo":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, record.Memo);
                            break; */

                        case "Url":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, Url);
                            break;

                        case "OutAccount":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, OutAccount);
                            break;

                        case "InAccount":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, InAccount);
                            break;

                        case "DebtAccount":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, DebtAccount);
                            break;

                        case "Account":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, Account);
                            break;

                        case "Price":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, Price);
                            break;

                        case "OriginalDate":
                            originalMemo = originalMemo.Replace(match.Groups[0].Value, OriginalDate);
                            break;
                    }
                }

                Memo = originalMemo;
            }
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
