using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PaySlipLoanTrnRepository : Repository<Pay_Slip_Loan_Trn>, IPaySlipLoanTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PaySlipLoanTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPaySlipLoanTrnAsync(Pay_Slip_Loan_Trn paySlipLoanTrn)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Slip_Loan_Trn.MaxAsync(x => x.Tr_Id);
                maxId++;
                paySlipLoanTrn.Tr_Id = maxId;
                await AddAsync(paySlipLoanTrn);
                await CSISContext.SaveChangesAsync(); // Save changes to reflect in the database
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Staff loan recovery in payroll not saved");
            }
            return result;
        }

        public async Task<bool> EditPaySlipLoanTrnAsync(Pay_Slip_Loan_Trn paySlipLoanTrn)
        {
            bool result = false;
            try
            {
                paySlipLoanTrn.LoanTr_Delete = true;
                await EditAsync(paySlipLoanTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Staff loan recovery in payroll not modified");
            }
            return result;
        }

        public async Task<List<Pay_Slip_Loan_Trn>> GetPaySlipLoanTrnList(decimal payId, decimal empId, string brCode)
        {
            List<Pay_Slip_Loan_Trn> loanList = new();
            try
            {
                var result = await  (from loan in CSISContext.Pay_Slip_Loan_Trn
                              join slip in CSISContext.Pay_Slip_Trn
                                  on new { loan.Loan_Id, loan.Pay_Id, loan.BrCode }
                                  equals new { slip.Loan_Id, slip.Pay_Id, slip.BrCode }
                              where loan.Pay_Id == payId &&
                                    slip.Pay_Id == payId &&
                                    slip.Mem_Id == empId &&
                                    loan.BrCode == brCode &&
                                    slip.BrCode == brCode
                              select loan)
             .ToListAsync();
                if(result != null && result.Any()) loanList = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return loanList;
        }

        public async Task<List<LoanDetailsVM>> GetPaySlipLoanList(decimal payId, decimal empId, string brCode)
        {
            List<LoanDetailsVM> loanList = new();
            try
            {
                var query =
                await (from psl in CSISContext.Pay_Slip_Loan_Trn
                join lm in CSISContext.Loan_Master on psl.Loan_Id equals lm.Loan_Id
                join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                where psl.Pay_Id == payId
                    && lm.Mem_Id == empId
                    && psl.LoanTr_Delete == false
                    && psl.BrCode == brCode
                    && lm.BrCode == brCode
                    && ls.BrCode == brCode
                select new LoanDetailsVM
                {
                    memid = lm.Mem_Id,
                    loanid = psl.Loan_Id,
                    loanno = lm.Loan_No,
                    intcollamt = psl.Int_Coll,
                    prlcoll = psl.Prl_Coll,
                    intcalcdate = psl.Int_Calc_Upto,
                    intcalcamt = psl.Int_Calc_Amt,
                    prlledid = ls.PrlLed_Id,
                    intledid = ls.IntLed_Id,
                    piledid = ls.PILed_Id,
                    prlschedule = psl.Prl_Schedule,
                    
                }).ToListAsync ();
                if (query != null && query.Any()) loanList = query.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return loanList;
        }
    }
}
