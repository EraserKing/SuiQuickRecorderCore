using SuiQuickRecorderCore.Models.Interfaces;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderCore.Models.Records;
using System;
using System.Collections.Generic;

namespace SuiQuickRecorderCore.Models
{
    public class SuiRecordFactory
    {
        public static SuiLineProcessResult Create(SuiRecordOrigin origin, SuiRecordReference reference, int l)
        {
            var lineResult = new SuiLineProcessResult { Line = l + 2 };

            foreach (var x in origin.SplitByDate())
            {
                try
                {
                    var recordType = x.GetRecordType(reference);

                    ISuiRecord record = recordType switch
                    {
                        SuiRecordType.Out => new SuiRecordOut(x, reference, recordType),
                        SuiRecordType.In => new SuiRecordIn(x, reference, recordType),
                        SuiRecordType.Transfer => new SuiRecordTransfer(x, reference, recordType),
                        SuiRecordType.Loan => new SuiRecordLoan(x, reference, recordType),
                        SuiRecordType.Combined => new SuiRecordCombined(x, reference, recordType),
                        // This branch should never be falled onto
                        _ => throw new ArgumentOutOfRangeException($"Cannot detect record type by category {x.Category}, or account {x.Account} & {x.Account2}")
                    };

                    lineResult.Records.Add(record);
                }
                catch (Exception ex)
                {
                    lineResult.Errors.Add(ex.InnerException?.Message ?? ex.Message);
                }
            }

            lineResult.Success = lineResult.Errors.Count == 0;
            return lineResult;
        }
    }
}
