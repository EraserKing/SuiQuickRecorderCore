using SuiQuickRecorderCore.Models.Interfaces;
using System.Collections.Generic;

namespace SuiQuickRecorderCore.Models
{
    public class SuiLineProcessResult
    {
        public int Line { get; set; }
        public bool Success { get; set; }
        public List<ISuiRecord> Records { get; set; } = new List<ISuiRecord>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
