using SuiQuickRecorderCore.Models.Origin;
using System.Collections.Generic;

namespace SuiQuickRecorderCore.Models.Records
{
    public class SuiRecordOut : SuiRecordTransactionBase
    {
        public string Category { get; set; }

        public SuiRecordOut(SuiRecordOrigin recordModel, SuiRecordReference reference, SuiRecordType recordType) : this(recordModel.Date, recordModel.Category, recordModel.Account, recordModel.Price, recordModel.Store, recordModel.Memo, reference, recordType)
        {

        }

        public SuiRecordOut(string date, string category, string account, string price, string store, string memo, SuiRecordReference reference, SuiRecordType recordType) : base(date, price, memo, recordType)
        {
            Category = reference.CategoriesOut[category];
            Account = reference.Accounts[account];
            if (!string.IsNullOrEmpty(store))
            {
                Store = reference.Stores[store];
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
