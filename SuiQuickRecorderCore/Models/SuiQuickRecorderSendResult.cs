using System.Collections.Generic;

namespace SuiQuickRecorderCore.Models
{
    public class SuiQuickRecorderSendResult
    {
        public int TotalRecords { get; set; }
        public int SuccessRecords { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessRequests { get; set; }
        public List<SuiLineProcessResult> Lines { get; set; } = new List<SuiLineProcessResult>();
    }
}
