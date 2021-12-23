using SuiQuickRecorderCore.Models.Records;
using System.Collections.Generic;

namespace SuiQuickRecorderCore.Models.Interfaces
{
    public interface ISuiRecord
    {
        IEnumerable<SuiRecordNetworkRequest> CreateNetworkRequests();

        SuiRecordType RecordType { get; set; }

        string Id { get; set; }
        string Store { get; set; }
        string Time { get; set; }
        string Project { get; set; }
        string Member { get; set; }
        string Memo { get; set; }
        string Url { get; set; }
        string OutAccount { get; set; }
        string InAccount { get; set; }
        string DebtAccount { get; set; }
        string Account { get; set; }
        string Price { get; set; }
        string OriginalDate { get; set; }
    }
}
