using SuiQuickRecorderCore.Models.Interfaces;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderCore.Models.Records;
using System;

namespace SuiQuickRecorderCore.Models
{
    public class SuiRecordFactory
    {
        public static ISuiRecord Create(SuiRecordOrigin origin, SuiRecordReference reference)
        {
            var recordType = origin.GetRecordType(reference);
            switch (recordType)
            {
                case SuiRecordType.Out:
                    return new SuiRecordOut(origin, reference, recordType);

                case SuiRecordType.In:
                    return new SuiRecordIn(origin, reference, recordType);

                case SuiRecordType.Transfer:
                    return new SuiRecordTransfer(origin, reference, recordType);

                case SuiRecordType.Loan:
                    return new SuiRecordLoan(origin, reference, recordType);

                // This branch should never be falled onto
                default:
                    throw new ArgumentOutOfRangeException($"Cannot detect record type by category {origin.Category}, or account {origin.Account} & {origin.Account2}");
            }
        }
    }
}
