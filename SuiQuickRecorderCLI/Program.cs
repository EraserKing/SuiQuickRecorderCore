﻿using SuiQuickRecorderCore;
using SuiQuickRecorderCore.Controllers;
using System;

namespace SuiQuickRecorderCLI
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
                LoanersFile = "loaners.csv",
                CookiesFile = "cookies.txt"
            });
            if(!controller.IsCredentialValid())
            {
                Console.WriteLine("Invalid credential, please re-enter cookies");
                return;
            }
            controller.LoadRecords("records.csv");

            foreach(string result in controller.SendLoadedRecords())
            {
                Console.WriteLine(result);
            }
        }
    }
}
