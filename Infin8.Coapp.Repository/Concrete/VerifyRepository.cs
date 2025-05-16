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
        public bool VerifyNomenClature(string accountNo, string accountType, int loanSchemeId, string brCode, out string errorMessage)
        {
            errorMessage = "";
            Boolean result = true;
            string beginString = "";
            try
            {
                switch (accountType)
                {
                    case "A-MEMBER":
                        if (accountNo.Substring(0, 5) != brCode)
                            errorMessage = "First Five degits not matched with pldb code";
                        beginString = "1";
                        if (accountNo.Length != 13)
                            errorMessage += "\n Length of Member No should be in 13";
                        if (accountNo.Substring(5, 1) != beginString)
                            errorMessage += "\n Member Number should start with 1 after pldb code";
                        break;
                    case "AM":
                        beginString = "2";
                        if (accountNo.Length != 13)
                            errorMessage += "\n Length of Associate Member No should be in 13";
                        if (accountNo.Substring(5, 1) != beginString)
                            errorMessage += "\n Associate Member Number should start with 2 after pldb code";
                        break;
                    case "NON-MEMBER":
                        beginString = "3";
                        if (accountNo.Length != 13)
                            errorMessage += "\n Length of Non-Member No should be in 13";
                        if (accountNo.Substring(5, 1) != beginString)
                            errorMessage += "\n Non-Member Number should start with 3 after pldb code";
                        break;
                    case "STAFF":
                        beginString = "4";
                        if (accountNo.Length != 13)
                            errorMessage += "\n Length of Staff No should be in 13";
                        if (accountNo.Substring(0, 1) != beginString)
                            errorMessage += "\n Staff Number should start with 4 after pldb code";
                        break;
                    case "FD":
                        beginString = "03";
                        if (accountNo.Length != 14)
                            errorMessage += "\n Length of FD No should be in 14 digits";
                        if (accountNo.Substring(5, 2) != beginString)
                            errorMessage += "\n FD Number should start with 03 after pldb code";
                        break;
                    case "RD":
                        beginString = "04";
                        if (accountNo.Length != 14)
                            errorMessage += "\n Length of RD No. should be in 14 digits";
                        if (accountNo.Substring(5, 2) != beginString)
                            errorMessage += "\n RD Number should start with 04 after pldb code";
                        break;
                    case "SB":
                        beginString = "01";
                        if (accountNo.Length != 14)
                            errorMessage += "\n Length of SB A/c No should be in 14 degits";
                        if (accountNo.Substring(0, 1) != beginString)
                            errorMessage += "\n SB Account Number should start with 01 after pldb code";
                        break;
                    case "CC":
                        beginString = "02";
                        if (accountNo.Length != 14)
                            errorMessage += "\n Length of Current No A/c should be in 14 digits";
                        if (accountNo.Substring(0, 1) != beginString)
                            errorMessage += "\n Current Account Number should start with 02 after pldb code";
                        break;
                    case "LOAN":
                        if (accountNo.Length != 15)
                            errorMessage += "\n Length of Loan No A/c should be in 15 digits";
                        string errorMessageFromLoan = "";

                        //LoanService lnService = new LoanService();
                        //beginString = lnService.GetLoanNoStartWithBySchemeId(loanSchemeId, out errorMessageFromLoan);
                        if (errorMessageFromLoan.Length > 0)
                            errorMessage += "\n " + errorMessageFromLoan;
                        if (accountNo.Substring(5, 3) != beginString)
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
