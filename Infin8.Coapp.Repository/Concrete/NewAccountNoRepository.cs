using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.SqlTypes;

namespace Infin8.Coapp.Repository
{
    public class NewAccountNoRepository : Repository<mem_master>, INewAccountNoRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public NewAccountNoRepository(DbContext context) : base(context)
        {
        }
        public async Task<string> NewMemberNoAsync(int memType, string brCode)
        {
            decimal MaxMemNo = 0;
            string MemNoString = "";
            try
            {
                //MaxMemNo = await CSISContext.Database.SqlQueryRaw<decimal>(
                //    @"SELECT MAX(CAST(REGEXP_REPLACE(SUBSTRING(memberNo FROM pos), '[^0-9]', '', 'g') AS decimal)) AS MemNo
                //        FROM (
                //            SELECT memberNo, POSITION('[0-9]' IN memberNo) AS pos
                //            FROM Mem_Master
                //            WHERE MemberDelete = false AND memberType = @memType AND BrCode = @brCode
                //        ) d"
                //    , new NpgsqlParameter("@memType", memType)
                //    , new NpgsqlParameter("@brCode", brCode)).FirstOrDefaultAsync();

                //MaxMemNo = await CSISContext.Database.SqlQueryRaw<decimal>( 
                //    @" SELECT MAX(CAST(REGEXP_REPLACE(SUBSTRING(memberNo FROM POSITION('[0-9]' IN memberNo)), '[^0-9]', '', 'g') AS decimal))
                //         FROM Mem_Master
                //         WHERE MemberDelete = false AND memberType = @memType AND BrCode = @brCode"
                //         , new NpgsqlParameter("@memType", memType)
                //         , new NpgsqlParameter("@brCode", brCode)).FirstOrDefaultAsync();

                //var maxMemNo = await CSISContext.Database.SqlQueryRaw<decimal>( 
                //    @"
                //        SELECT MAX(
                //            CAST(
                //                REGEXP_REPLACE(
                //                    SUBSTRING(memberNo FROM POSITION('[0-9]' IN memberNo)), 
                //                    '[^0-9]', '', 'g'
                //                ) AS decimal
                //            )
                //        ) AS MemNo
                //        FROM Mem_Master
                //        WHERE MemberDelete = false AND memberType = @memType AND BrCode = @brCode",
                //        new NpgsqlParameter("@memType", memType),
                //        new NpgsqlParameter("@brCode", brCode)
                //    .Select(x => x.MemNo))  // Assuming your entity has a MemNo property.
                //    .FirstOrDefaultAsync();

                var newMemNo = await CSISContext.mem_master
                .FromSqlRaw(@"
                    SELECT MAX(
                        CAST(
                            REGEXP_REPLACE(
                                SUBSTRING(memberno FROM POSITION('[0-9]' IN memberno)), 
                                '[^0-9]', '', 'g'
                            ) AS varchar
                        )
                    ) AS memberno
                    FROM Mem_Master
                    WHERE MemberDelete = false AND memberType = @memType AND BrCode = @brCode",
                    new NpgsqlParameter("@memType", memType),
                    new NpgsqlParameter("@brCode", brCode)
                )
                .Select(x => x.memberno)  // Assuming your entity has a MemNo property.
                .FirstOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(newMemNo))
                {
                    MaxMemNo = Convert.ToDecimal(newMemNo);
                }
                if (MaxMemNo == 0)
                {
                    switch (memType)
                    {
                        case 1:
                            MaxMemNo = Convert.ToDecimal (brCode + (10000001).ToString());
                            break;
                        case 2:
                            MaxMemNo = Convert.ToDecimal(brCode + (20000001).ToString()); // 20000001;
                            break;
                        case 3:
                            MaxMemNo = Convert.ToDecimal(brCode + (30000001).ToString()); // 30000001;
                            break;
                        case 4:
                            MaxMemNo = Convert.ToDecimal(brCode + (40000001).ToString()); // 40000001;
                            break;
                    }
                }
                else
                    MaxMemNo++;
            }
            catch (Exception ex) 
            {
                string errormessage = ex.Message;
                MaxMemNo = 0;
            }
            switch (memType)
            {
                case 1:
                    MemNoString = MaxMemNo.ToString("#############");
                    break;
                case 2:
                    MemNoString = brCode + MaxMemNo.ToString("########");
                    break;
                case 3:
                    MemNoString = brCode + MaxMemNo.ToString("########");
                    break;
                case 4:
                    MemNoString = brCode + MaxMemNo.ToString("########");
                    break;
            }
            return MemNoString;
        }
        public async Task<string> NewLoanNoAsync(int schemeId, string brCode)
        {
            string loanNo = "";
            int lengthOfLoanNo = 15;
            try
            {
                var maxLoanNo =await  (from loan in CSISContext.Loan_Master
                                 where loan.Scheme_Id == 1 && loan.BrCode == brCode
                                 select loan.Loan_No).MaxAsync();
                if (maxLoanNo != null) loanNo = maxLoanNo.ToString();

                if (!string.IsNullOrWhiteSpace(loanNo))
                {
                    long n;
                    bool isNumeric = long.TryParse(loanNo, out n);
                    if (isNumeric)
                    {
                        if (loanNo.Length < lengthOfLoanNo)
                        {
                            //errorMessage = "Loan No should have " + lengthOfLoanNo + " degits. Existing Loan No had only " + (loanNo.Length).ToString() + " digits";
                        }
                        loanNo = (Convert.ToInt64(loanNo) + 1).ToString();
                    }
                    else
                    {
                        //errorMessage = "Existing Loan No found as non-numeric";
                    }
                }
                else
                {
                    var NoStartWith = await  (from scheme in CSISContext.Loan_Schemes
                                       where scheme.Scheme_Id == schemeId && scheme.BrCode == brCode
                                       select scheme.LoanNoStartWith).FirstOrDefaultAsync();
                    string loanNoStartWith = "";
                    if (NoStartWith != null) loanNoStartWith = NoStartWith.ToString();
                    //if (NoStartWith != null) loanNo = maxLoanNo2.ToQueryString();

                    if (!string.IsNullOrWhiteSpace(loanNoStartWith))
                    {
                        loanNo = brCode + loanNoStartWith + "0000001";
                    }
                    else
                    {
                        loanNo = "Loan No missing";
                    }
                }

            }
            catch (Exception )
            {
                //errorMessage = ex.Message + "\n Error in obtain New Loan No";
            }
            return loanNo;
        }
        public async Task<string> NewTDNoAsync(int schemeId, string brCode)
        {
            //errorMessage = "";
            string maxNo = "";
            try
            {
                //if (schemeId < 40000)
                //{
                    var maxTdNo = await (from td in CSISContext.TermDeposit_Master
                                   where td.TDScheme_Id == schemeId && td.BrCode == brCode
                                   select td.TD_No).MaxAsync();
                    if (maxTdNo == null)
                    {
                        if (schemeId < Convert.ToInt32(brCode + 400)) maxNo = Convert.ToInt16(brCode) + "030000001";
                        if (schemeId >= Convert.ToInt32(brCode + 400) && schemeId < Convert.ToInt16(brCode + 500)) maxNo = brCode + "040000001";
                        if (schemeId >= Convert.ToInt32(brCode + 500)) maxNo = brCode + "050000001";
                    }
                    else
                    {
                        long.TryParse(maxTdNo, out long newMaxNo);
                        newMaxNo++;
                        maxNo = newMaxNo.ToString();
                    }
                //}
            }
            catch (Exception)
            {
                //errorMessage = ex.Message;
            }
            return maxNo;
        }
        public async Task<(string newSBNo, string errorMessage)> NewSBNoAsyncWithErrorMessage(int schemeId, string brCode)
        {
            string errorMessage = "";
            string newSBNo = "";
            string maxNo = "";
            try
            {
                var maxAccNo = await  (from td in CSISContext.SBCA_Master
                                where td.Scheme_Id == schemeId && td.BrCode == brCode
                                select td.Acc_No).MaxAsync();

                if (maxAccNo == null)
                {
                    if (schemeId == 7) maxNo = brCode + "010000001";
                    if (schemeId == 8) maxNo = brCode + "020000001";
                }
                else
                {
                    int.TryParse(maxNo, out int newMaxNo);
                    newMaxNo++;
                    maxNo = newMaxNo.ToString();
                }
                newSBNo = maxNo;

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return (newSBNo , errorMessage);
        }

        public async Task<string> NewSBNoAsync(int schemeId, string brCode)
        {
            string newSBNo = "";
            string maxNo = "";
            try
            {
                var maxAccNo = await(from td in CSISContext.SBCA_Master
                                     where td.Scheme_Id == schemeId && td.BrCode == brCode
                                     select td.Acc_No).MaxAsync();

                if (maxAccNo == null)
                {
                    if (schemeId == 7) maxNo = brCode + "010000001";
                    if (schemeId == 8) maxNo = brCode + "020000001";
                }
                else
                {
                    int.TryParse(maxNo, out int newMaxNo);
                    newMaxNo++;
                    maxNo = newMaxNo.ToString();
                }
                newSBNo = maxNo;

            }
            catch (Exception )
            {
                throw;
            }
            return newSBNo;
        }
    }
}

