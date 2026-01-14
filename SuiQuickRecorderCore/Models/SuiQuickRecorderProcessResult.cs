using System.Collections.Generic;

namespace SuiQuickRecorderCore.Models
{
    public class SuiQuickRecorderProcessResult
    {
        public int TotalRecords { get; set; }
        public int SuccessRecords { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessRequests { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
