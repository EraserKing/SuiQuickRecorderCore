using System;

namespace SuiQuickRecorderCore.Models.Origin
{
    public class SuiRecordOrigin
    {
        public string Date { get; set; }
        public string Category { get; set; }
        public string Account { get; set; }
        public string Account2 { get; set; }
        public string Price { get; set; }
        public string Store { get; set; }
        public string Memo { get; set; }

        public SuiRecordType GetRecordType(SuiRecordReference reference)
        {
            if (reference.Accounts.Contains(Account) && reference.Accounts.Contains(Account2))
            {
                return SuiRecordType.Transfer;
            }

            if (reference.Accounts.Contains(Account) && string.IsNullOrEmpty(Account2))
            {
                if (reference.CategoriesIn.Contains(Category))
                {
                    return SuiRecordType.In;
                }

                if (reference.CategoriesOut.Contains(Category))
                {
                    return SuiRecordType.Out;
                }
            }

            if (!string.IsNullOrEmpty(Account) && !string.IsNullOrEmpty(Account2))
            {
                if (Account.StartsWith(">>") && !Account2.StartsWith(">>") && reference.Loaners.Contains(Account.Substring(2)) && reference.Accounts.Contains(Account2))
                {
                    return SuiRecordType.Loan;
                }

                if (Account2.StartsWith(">>") && !Account.StartsWith(">>") && reference.Loaners.Contains(Account2.Substring(2)) && reference.Accounts.Contains(Account))
                {
                    return SuiRecordType.Loan;
                }
            }

            throw new ArgumentOutOfRangeException($"Cannot detect record type by category {Category}, or account {Account} & {Account2}");
        }
    }
}
