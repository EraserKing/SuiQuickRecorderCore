using System;
using System.Collections.Generic;
using System.Text;

namespace SuiQuickRecorderCore.Models
{
    public class SuiRecordIn : SuiRecordBase
    {
        public string Category { get; set; }
        public string Price2 { get; set; } = "";

        public SuiRecordIn(SuiRecordOrigin recordModel, SuiKVPairs categoriesIn, SuiKVPairs accounts, SuiKVPairs stores) : this(recordModel.Date, recordModel.Category, recordModel.Account, recordModel.Price, recordModel.Store, recordModel.Memo, categoriesIn, accounts, stores)
        {

        }

        public SuiRecordIn(string date, string category, string account, string price, string store, string memo, SuiKVPairs categoriesIn, SuiKVPairs accounts, SuiKVPairs stores) : base(date, price, memo)
        {
            Category = categoriesIn[category];
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
            record.Add(new KeyValuePair<string, string>("price2", Price2));
            return record;
        }

        public override string GetNetworkRequestEndpoint() => "tally/income.rmi";
    }
}
