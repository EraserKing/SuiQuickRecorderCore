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
            return recordType switch
            {
                SuiRecordType.Out => new SuiRecordOut(origin, reference, recordType),
                SuiRecordType.In => new SuiRecordIn(origin, reference, recordType),
                SuiRecordType.Transfer => new SuiRecordTransfer(origin, reference, recordType),
                SuiRecordType.Loan => new SuiRecordLoan(origin, reference, recordType),
                SuiRecordType.Combined => new SuiRecordCombined(origin, reference, recordType),
                // This branch should never be falled onto
                _ => throw new ArgumentOutOfRangeException($"Cannot detect record type by category {origin.Category}, or account {origin.Account} & {origin.Account2}"),
            };
        }
    }
}
