using SuiQuickRecorderCore;
using SuiQuickRecorderCore.Controllers;
using System;

namespace SuiQuickRecorderCUI
{
    class Program
    {
        static void Main(string[] args)
        {
            SuiQuickRecorderController controller = new SuiQuickRecorderController(new SuiQuickRecorderControllerOptions
            {
                AccountsFile = "dataAccounts.csv",
                CategoriesInFile = "dataCategoriesIn.csv",
                CategoriesOutFile = "dataCategoriesOut.csv",
                StoresFile = "dataStores.csv",
                CookiesFile = "cookies.txt"
            });
            if(!controller.IsCredentialValid())
            {
                Console.WriteLine("Invalid credential, please re-enter cookies");
                return;
            }
            controller.LoadRecords("records.csv");
            Console.Write(controller.SendLoadedRecords());
        }
    }
}
