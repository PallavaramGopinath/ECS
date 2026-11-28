using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    public class VerifyRepository : Repository<mem_master> , IVerifyRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public VerifyRepository(DbContext context) : base(context)
        {

        }
        public bool VerifyNomenClature(string accountNo, string accountType, long loanSchemeId, string brCode, out string errorMessage)
        {
            errorMessage = "";
            Boolean result = true;
            string beginString = "";
            int prefixLength = brCode.Length;
            try
            {
                switch (accountType)
                {
                    case "A-MEMBER":
                        if (!accountNo.StartsWith(brCode, StringComparison.Ordinal))
                            errorMessage = "Account number does not start with the branch code";
                        beginString = "1";
                        if (accountNo.Length != prefixLength + 8)
                            errorMessage += $"\n Member No must have {prefixLength + 8} digits";
                        if (accountNo.Substring(prefixLength, 1) != beginString)
                            errorMessage += "\n Member Number should start with 1 after pldb code";
                        break;
                    case "AM":
                        beginString = "2";
                        if (accountNo.Length != prefixLength + 8)
                            errorMessage += $"\n Associate Member No must have {prefixLength + 8} digits";
                        if (accountNo.Substring(prefixLength, 1) != beginString)
                            errorMessage += "\n Associate Member Number should start with 2 after pldb code";
                        break;
                    case "NON-MEMBER":
                        beginString = "3";
                        if (accountNo.Length != prefixLength + 8)
                            errorMessage += $"\n Non-Member No must have {prefixLength + 8} digits";
                        if (accountNo.Substring(prefixLength, 1) != beginString)
                            errorMessage += "\n Non-Member Number should start with 3 after pldb code";
                        break;
                    case "STAFF":
                        beginString = "4";
                        if (accountNo.Length != prefixLength + 8)
                            errorMessage += $"\n Staff No must have {prefixLength + 8} digits";
                        if (accountNo.Substring(prefixLength, 1) != beginString)
                            errorMessage += "\n Staff Number should start with 4 after pldb code";
                        break;
                    case "FD":
                        beginString = "03";
                        if (accountNo.Length != prefixLength + 9)
                            errorMessage += $"\n FD No must have {prefixLength + 9} digits";
                        if (accountNo.Substring(prefixLength, 2) != beginString)
                            errorMessage += "\n FD Number should start with 03 after pldb code";
                        break;
                    case "RD":
                        beginString = "04";
                        if (accountNo.Length != prefixLength + 9)
                            errorMessage += $"\n RD No must have {prefixLength + 9} digits";
                        if (accountNo.Substring(prefixLength, 2) != beginString)
                            errorMessage += "\n RD Number should start with 04 after pldb code";
                        break;
                    case "SB":
                        beginString = "01";
                        if (accountNo.Length != prefixLength + 9)
                            errorMessage += $"\n SB Account No must have {prefixLength + 9} digits";
                        if (accountNo.Substring(prefixLength, 2) != beginString)
                            errorMessage += "\n SB Account Number should start with 01 after pldb code";
                        break;
                    case "CC":
                        beginString = "02";
                        if (accountNo.Length != prefixLength + 9)
                            errorMessage += $"\n Current Account No must have {prefixLength + 9} digits";
                        if (accountNo.Substring(prefixLength, 2) != beginString)
                            errorMessage += "\n Current Account Number should start with 02 after pldb code";
                        break;
                    case "LOAN":
                        if (accountNo.Length != prefixLength + 10)
                            errorMessage += $"\n Loan No must have {prefixLength + 10} digits";
                        string errorMessageFromLoan = "";

                        //LoanService lnService = new LoanService();
                        //beginString = lnService.GetLoanNoStartWithBySchemeId(loanSchemeId, out errorMessageFromLoan);
                        if (errorMessageFromLoan.Length > 0)
                            errorMessage += "\n " + errorMessageFromLoan;
                        if (beginString.Length > 0 && accountNo.Substring(prefixLength, 3) != beginString)
                            errorMessage += "\n Loan No should start with " + beginString + " after pldb code";
                        break;
                }
                if (errorMessage.Length > 0)
                    result = false;
            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
            }
            return result;
        }

        public bool VerifyDuplicateAccountNo(string accountNo, string tableName, string searchField, string deleteField)
        {
            bool result = true;
            int count = 0;
            try
            {
                count = CSISContext.Database.SqlQueryRaw<int>(
                    @"SELECT Count(*)  FROM " + tableName + " WHERE " + searchField + " = '" + accountNo + "' AND " + deleteField + " = False").FirstOrDefault();
            }
            catch
            {
                count = 0;
            }
            if (count > 0)
                result = true;
            else
                result = false;
            return result;
        }
    }
}
