using System;
using System.Collections.Generic;
using System.Text;

namespace SuiQuickRecorderCore.Models
{
    public class SuiRecordTransfer : SuiRecordBase
    {
        public string Price2 { get; set; }

        public SuiRecordTransfer(SuiRecordOrigin recordModel, SuiKVPairs accounts) : this(recordModel.Date, recordModel.Account, recordModel.Account2, recordModel.Price, recordModel.Memo, accounts)
        {

        }

        public SuiRecordTransfer(string date, string accountOut, string accountIn, string price, string memo, SuiKVPairs accounts) : base(date, price, memo)
        {
            OutAccount = accounts[accountOut];
            InAccount = accounts[accountIn];
        }

        public override List<KeyValuePair<string, string>> ToNetworkRequestBody()
        {
            var record = base.ToNetworkRequestBody();
            record.Add(new KeyValuePair<string, string>("price2", Price2));
            return record;
        }

        public override string GetNetworkRequestEndpoint() => "tally/transfer.rmi";
    }
}
