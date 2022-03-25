using SuiQuickRecorderCore.Models.Origin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuiQuickRecorderCore.Models.Records
{
    public class SuiRecordCombined : SuiRecordTransactionBase
    {
        public string Category { get; set; }
        public string Price2 { get; set; }

        private SuiRecordReference _reference { get; set; }

        public SuiRecordType SplitRecordType { get; private set; }

        public SuiRecordCombined(SuiRecordOrigin recordModel, SuiRecordReference reference, SuiRecordType recordType) : this(recordModel.Date, recordModel.Category, recordModel.Account, recordModel.Account2, recordModel.Price, recordModel.Store, recordModel.Memo, reference, recordType)
        {

        }

        public SuiRecordCombined(string date, string category, string accountOut, string accountIn, string price, string store, string memo, SuiRecordReference reference, SuiRecordType recordType) : base(date, price, memo, recordType)
        {
            _reference = reference;
            Category = category;
            if (reference.CategoriesIn.Contains(category))
            {
                SplitRecordType = SuiRecordType.In;
            }
            else if (reference.CategoriesOut.Contains(category))
            {
                SplitRecordType = SuiRecordType.Out;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(category), $"Cannot distinguish category from {category}");
            }

            OutAccount = accountOut;
            InAccount = accountIn;

            Store = store;
            UpdateMemo();
        }

        public new List<KeyValuePair<string, string>> ToNetworkRequestBody()
        {
            throw new NotImplementedException("This method should not be directly called for LOAN type");
        }

        public override string GetNetworkRequestEndpoint()
        {
            throw new NotImplementedException("This method should not be directly called for LOAN type");
        }

        public override IEnumerable<SuiRecordNetworkRequest> CreateNetworkRequests()
        {
            List<SuiRecordNetworkRequest> requests = new List<SuiRecordNetworkRequest>();

            requests.AddRange(new SuiRecordTransfer(OriginalDate, OutAccount, InAccount, Price.Split(",")[1], Memo, _reference, SuiRecordType.Transfer).CreateNetworkRequests());

            switch (SplitRecordType)
            {
                case SuiRecordType.In:
                    requests.AddRange(new SuiRecordIn(OriginalDate, Category, InAccount, Price.Split(",")[0], Store, Memo, _reference, SplitRecordType).CreateNetworkRequests());
                    break;

                case SuiRecordType.Out:
                    requests.AddRange(new SuiRecordOut(OriginalDate, Category, OutAccount, Price.Split(",")[0], Store, Memo, _reference, SplitRecordType).CreateNetworkRequests());
                    break;
            }

            return requests;
        }
    }
}
