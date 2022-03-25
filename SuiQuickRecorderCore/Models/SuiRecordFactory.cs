using SuiQuickRecorderCore.Models.Interfaces;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderCore.Models.Records;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuiQuickRecorderCore.Models
{
    public class SuiRecordFactory
    {
        public static IEnumerable<ISuiRecord> Create(SuiRecordOrigin origin, SuiRecordReference reference)
        {
            return origin.SplitByDate().Select<SuiRecordOrigin, ISuiRecord>(x =>
            {
                var recordType = x.GetRecordType(reference);

                return recordType switch
                {
                    SuiRecordType.Out => new SuiRecordOut(x, reference, recordType),
                    SuiRecordType.In => new SuiRecordIn(x, reference, recordType),
                    SuiRecordType.Transfer => new SuiRecordTransfer(x, reference, recordType),
                    SuiRecordType.Loan => new SuiRecordLoan(x, reference, recordType),
                    SuiRecordType.Combined => new SuiRecordCombined(x, reference, recordType),
                    // This branch should never be falled onto
                    _ => throw new ArgumentOutOfRangeException($"Cannot detect record type by category {x.Category}, or account {x.Account} & {x.Account2}")
                };
            });
        }
    }
}
