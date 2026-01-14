namespace SuiQuickRecorderCore.Models.Options
{
    public class SuiQuickRecorderServiceOptions
    {
        public string ProxyHost { get; set; }
        public int? ProxyPort { get; set; } = null;
        public string ApiBaseUrl { get; set; } = "https://www.sui.com";
    }
}
