using System;
using System.Collections.Generic;
using System.Text;

namespace SuiQuickRecorderCore.Models
{
    public class SuiRecordOrigin
    {
        public string Date { get; set; }
        public string Category { get; set; }
        public string Account { get; set; }
        public string Account2 { get; set; }
        public string Price { get; set; }
        public string Store { get; set; }
        public string Memo { get; set; }

        public SuiRecordType GetRecordType(SuiKVPairs accounts, SuiKVPairs categoriesIn, SuiKVPairs categoriesOut)
        {
            if (accounts.Contains(Account) && accounts.Contains(Account2))
            {
                return SuiRecordType.Transfer;
            }

            if (accounts.Contains(Account) && string.IsNullOrEmpty(Account2))
            {
                if (categoriesIn.Contains(Category))
                {
                    return SuiRecordType.In;
                }

                if (categoriesOut.Contains(Category))
                {
                    return SuiRecordType.Out;
                }
            }

            throw new ArgumentException("Unable to detect record type");
        }
    }
}
