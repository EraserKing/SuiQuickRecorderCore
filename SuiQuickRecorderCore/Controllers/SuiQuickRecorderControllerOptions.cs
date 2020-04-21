using System;
using System.Collections.Generic;
using System.Text;

namespace SuiQuickRecorderCore.Controllers
{
    public class SuiQuickRecorderControllerOptions
    {
        public string AccountsFile { get; set; }
        public string CategoriesInFile { get; set; }
        public string CategoriesOutFile { get; set; }
        public string StoresFile { get; set; }
        public string CookiesFile { get; set; }
        public string LoanersFile { get; set; }
        public string ProxyHost { get; set; }
        public int? ProxyPort { get; set; } = null;
        public string ApiBaseUrl { get; set; } = "https://www.sui.com";
    }
}
