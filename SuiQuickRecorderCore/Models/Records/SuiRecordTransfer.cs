using SuiQuickRecorderCore.Models.Origin;
using System.Collections.Generic;

namespace SuiQuickRecorderCore.Models.Records
{
    public class SuiRecordTransfer : SuiRecordTransactionBase
    {
        public string Price2 { get; set; }

        public SuiRecordTransfer(SuiRecordOrigin recordModel, SuiRecordReference reference, SuiRecordType recordType) : this(recordModel.Date, recordModel.Account, recordModel.Account2, recordModel.Price, recordModel.Memo, reference, recordType)
        {

        }

        public SuiRecordTransfer(string date, string accountOut, string accountIn, string price, string memo, SuiRecordReference reference, SuiRecordType recordType) : base(date, price, memo, recordType)
        {
            OutAccount = reference.Accounts[accountOut];
            InAccount = reference.Accounts[accountIn];
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
