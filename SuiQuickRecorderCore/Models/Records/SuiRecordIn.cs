using SuiQuickRecorderCore.Models.Origin;
using System.Collections.Generic;

namespace SuiQuickRecorderCore.Models.Records
{
    public class SuiRecordIn : SuiRecordTransactionBase
    {
        public string Category { get; set; }
        public string Price2 { get; set; } = "";

        public SuiRecordIn(SuiRecordOrigin recordModel, SuiRecordReference reference, SuiRecordType recordType) : this(recordModel.Date, recordModel.Category, recordModel.Account, recordModel.Price, recordModel.Store, recordModel.Memo, reference, recordType)
        {

        }

        public SuiRecordIn(string date, string category, string account, string price, string store, string memo, SuiRecordReference reference, SuiRecordType recordType) : base(date, price, memo, recordType)
        {
            Category = reference.CategoriesIn[category];
            Account = reference.Accounts[account];
            if (!string.IsNullOrEmpty(store))
            {
                Store = reference.Stores[store];
            }
            UpdateMemo();
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
