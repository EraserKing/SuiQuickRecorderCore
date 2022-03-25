using SuiQuickRecorderCore.Models.Origin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SuiQuickRecorderCore.Models.Records
{
    public enum SuiRecordLoanDirection
    {
        Out,
        In,
        OutIn,
        InOut,
        None
    }

    public class SuiRecordLoan : SuiRecordTransactionBase
    {
        public string Price2 { get; set; }

        public string Opt { get; set; } = "setTransRelation";
        public string Type { get; set; }
        public string DebtId { get; set; }
        public string NewTranId { get; set; }
        public string OldTranId { get; set; }

        public string APAccount { get; set; }
        public string ARAccount { get; set; }

        public string Loaner { get; set; }
        public string Tag { get; set; }
        public string InPrice { get; set; }
        public string OutPrice { get; set; }

        public SuiRecordLoanDirection Direction { get; set; }

        public SuiRecordLoan(SuiRecordOrigin recordModel, SuiRecordReference reference, SuiRecordType recordType) : this(recordModel.Date, recordModel.Account, recordModel.Account2, recordModel.Price, recordModel.Memo, reference, recordType)
        {

        }

        public SuiRecordLoan(string date, string account, string account2, string price, string memo, SuiRecordReference reference, SuiRecordType recordType) : base(date, price, memo.Substring(memo.IndexOf(':') + 1), recordType)
        {
            APAccount = reference.Accounts["A-P"];
            ARAccount = reference.Accounts["A-R"];

            if (account.StartsWith(">>") && !account2.StartsWith(">>")) // IN
            {
                InAccount = reference.Accounts[account2];
                InPrice = price;

                Loaner = reference.Loaners.GetOfficialName(account.Substring(2));
                DebtId = reference.Loaners[account.Substring(2)];
                Direction = SuiRecordLoanDirection.In;
            }

            else if (!account.StartsWith(">>") && account2.StartsWith(">>")) // OUT
            {
                OutAccount = reference.Accounts[account];
                OutPrice = price;

                Loaner = reference.Loaners.GetOfficialName(account2.Substring(2));
                DebtId = reference.Loaners[account2.Substring(2)];
                Direction = SuiRecordLoanDirection.Out;
            }

            Price2 = price;
            if (!memo.Contains(":"))
            {
                throw new ArgumentOutOfRangeException("Loan record tag is not found");
            }
            Tag = memo.Substring(0, memo.IndexOf(':'));
            UpdateMemo();
        }

        public static IEnumerable<SuiRecordLoan> AutoCombine(IEnumerable<SuiRecordLoan> loans)
        {
            return loans.GroupBy(x => (x.Tag, x.DebtId)).Select(x => x.First().CombineLoan(x.Last())).Where(x => x.Direction != SuiRecordLoanDirection.None);
        }

        public SuiRecordLoan CombineLoan(SuiRecordLoan loan)
        {
            if (Direction == SuiRecordLoanDirection.None)
            {
                throw new Exception();
            }

            if (Direction == SuiRecordLoanDirection.Out && loan.Direction == SuiRecordLoanDirection.In)
            {
                if (DebtId == loan.DebtId && Tag == loan.Tag)
                {
                    Direction = SuiRecordLoanDirection.OutIn;
                    loan.Direction = SuiRecordLoanDirection.None;

                    InPrice = loan.InPrice;
                    InAccount = loan.InAccount;
                }
            }

            if (Direction == SuiRecordLoanDirection.In && loan.Direction == SuiRecordLoanDirection.Out)
            {
                if (DebtId == loan.DebtId && Tag == loan.Tag)
                {
                    Direction = SuiRecordLoanDirection.InOut;
                    loan.Direction = SuiRecordLoanDirection.None;

                    OutPrice = loan.OutPrice;
                    OutAccount = loan.OutAccount;
                }
            }

            return this;
        }

        public new List<KeyValuePair<string, string>> ToNetworkRequestBody()
        {
            throw new NotImplementedException("This method should not be directly called for LOAN type");
        }

        public override string GetNetworkRequestEndpoint()
        {
            throw new NotImplementedException("This method should not be directly called for LOAN type");
        }

        private SuiRecordNetworkRequest CreateTransferRequest(string time, string project, string member, string memo, string url, string outAccount, string inAccount, string price)
        {
            return new SuiRecordNetworkRequest("tally/transfer.rmi", new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("store", ""),
                    new KeyValuePair<string, string>("time", time),
                    new KeyValuePair<string, string>("project", project),
                    new KeyValuePair<string, string>("member", member),
                    new KeyValuePair<string, string>("memo", memo),
                    new KeyValuePair<string, string>("url", url),
                    new KeyValuePair<string, string>("debt", ""),
                    new KeyValuePair<string, string>("out_account", outAccount),
                    new KeyValuePair<string, string>("in_account", inAccount),
                    new KeyValuePair<string, string>("debt_account", ""),
                    new KeyValuePair<string, string>("account", ""),
                    new KeyValuePair<string, string>("price", price),
                    new KeyValuePair<string, string>("price2", price),
                });
        }

        private SuiRecordNetworkRequest CreateLoanRequest(string opt, string type, string debtId, string newTranId, string oldTranId)
        {
            return new SuiRecordNetworkRequest("fresh/debt.rmi", new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("opt", opt),
                    new KeyValuePair<string, string>("type", type),
                    new KeyValuePair<string, string>("debtId", debtId),
                    new KeyValuePair<string, string>("newTranId", newTranId),
                    new KeyValuePair<string, string>("oldTranId", oldTranId)
                });
        }

        public override IEnumerable<SuiRecordNetworkRequest> CreateNetworkRequests()
        {
            if (Direction == SuiRecordLoanDirection.None)
            {
                yield break;
            }

            if (Direction == SuiRecordLoanDirection.Out || Direction == SuiRecordLoanDirection.OutIn) // [OUT]
            {
                // TRANSFER
                var request1 = CreateTransferRequest(Time, Project, Member, $"[借出]{Loaner} {Memo}", Url, OutAccount, ARAccount, OutPrice);
                yield return request1;
                string outId = Regex.Match(request1.ResponseMessage.Result.Content.ReadAsStringAsync().Result, $@"outId:(\d+)").Groups[1].Value;

                // DEBT
                var request2 = CreateLoanRequest(Opt, "jiechu", DebtId, outId, "0");
                yield return request2;

                if (Direction == SuiRecordLoanDirection.OutIn) // OUT+[IN]
                {
                    // TRANSFER
                    var request3 = CreateTransferRequest(Time, Project, Member, $"[收债]{Loaner} {Memo}", Url, ARAccount, InAccount, InPrice);
                    yield return request3;
                    string outId2 = Regex.Match(request3.ResponseMessage.Result.Content.ReadAsStringAsync().Result, $@"outId:(\d+)").Groups[1].Value;

                    // DEBT
                    var request4 = CreateLoanRequest(Opt, "huanru", DebtId, outId2, outId);
                    yield return request4;
                }
            }

            if (Direction == SuiRecordLoanDirection.In || Direction == SuiRecordLoanDirection.InOut) // [IN]
            {
                // TRANSFER
                var request1 = CreateTransferRequest(Time, Project, Member, $"[借入]{Loaner} {Memo}", Url, APAccount, InAccount, OutPrice);
                yield return request1;
                string outId = Regex.Match(request1.ResponseMessage.Result.Content.ReadAsStringAsync().Result, $@"outId:(\d+)").Groups[1].Value;

                // DEBT
                var request2 = CreateLoanRequest(Opt, "jieru", DebtId, outId, "0");
                yield return request2;

                if (Direction == SuiRecordLoanDirection.InOut) // IN+[OUT]
                {
                    // TRANSFER
                    var request3 = CreateTransferRequest(Time, Project, Member, $"[还债]{Loaner} {Memo}", Url, OutAccount, APAccount, InPrice);
                    yield return request3;
                    string outId2 = Regex.Match(request3.ResponseMessage.Result.Content.ReadAsStringAsync().Result, $@"outId:(\d+)").Groups[1].Value;

                    // DEBT
                    var request4 = CreateLoanRequest(Opt, "huanchu", DebtId, outId2, outId);
                    yield return request4;
                }
            }
        }
    }
}
