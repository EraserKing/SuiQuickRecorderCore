using System;
using System.Collections.Generic;
using System.Text;

namespace SuiQuickRecorderCore.Models
{
    public class SuiRecordOut : SuiRecordBase
    {
        public string Category { get; set; }

        public SuiRecordOut(SuiRecordOrigin recordModel, SuiKVPairs categoriesOut, SuiKVPairs accounts, SuiKVPairs stores) : this(recordModel.Date, recordModel.Category, recordModel.Account, recordModel.Price, recordModel.Store, recordModel.Memo, categoriesOut, accounts, stores)
        {

        }

        public SuiRecordOut(string date, string category, string account, string price, string store, string memo, SuiKVPairs categoriesOut, SuiKVPairs accounts, SuiKVPairs stores) : base(date, price, memo)
        {
            Category = categoriesOut[category];
            Account = accounts[account];
            if (!string.IsNullOrEmpty(store))
            {
                Store = stores[store];
            }
        }

        public override List<KeyValuePair<string, string>> ToNetworkRequestBody()
        {
            var record = base.ToNetworkRequestBody();
            record.Add(new KeyValuePair<string, string>("category", Category));
            return record;
        }

        public override string GetNetworkRequestEndpoint() => "tally/payout.rmi";
    }
}
