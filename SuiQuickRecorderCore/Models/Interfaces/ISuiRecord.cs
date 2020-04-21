using SuiQuickRecorderCore.Models.Records;
using System.Collections.Generic;

namespace SuiQuickRecorderCore.Models.Interfaces
{
    public interface ISuiRecord
    {
        IEnumerable<SuiRecordNetworkRequest> CreateNetworkRequests();

        SuiRecordType RecordType { get; set; }
    }
}
