using System;
using System.Collections.Generic;
using System.Text;

namespace SuiQuickRecorderCore.Models
{
    public class SuiRecordFactory
    {
        public static ISuiRecord Create(SuiRecordOrigin origin, SuiKVPairs accounts, SuiKVPairs categoriesIn, SuiKVPairs categoriesOut, SuiKVPairs stores)
        {
            switch (origin.GetRecordType(accounts, categoriesIn, categoriesOut))
            {
                case SuiRecordType.Out:
                    return new SuiRecordOut(origin, categoriesOut, accounts, stores);

                case SuiRecordType.In:
                    return new SuiRecordIn(origin, categoriesIn, accounts, stores);

                case SuiRecordType.Transfer:
                    return new SuiRecordTransfer(origin, accounts);

                default:
                    throw new ArgumentOutOfRangeException($"Cannot detect record type by category {origin.Category}, or account {origin.Account} & {origin.Account2}");
            }
        }
    }
}
