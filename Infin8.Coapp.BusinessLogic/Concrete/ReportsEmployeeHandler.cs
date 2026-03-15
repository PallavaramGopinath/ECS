using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class ReportsEmployeeHandler : IReportsEmployeeHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IUtilityHandler _utilityHandler;
        public ReportsEmployeeHandler(IUnitOfWork unitOfWork, IUtilityHandler utilityHandler)
        {
            _unitOfWork = unitOfWork;
            _utilityHandler = utilityHandler;
        }

        public async Task<List<rptEmp12MonthsSalary>> GetEmployee12MonthsSalary(DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsEmployee.GetEmployee12MonthsSalary(fromDate, toDate, brCode);
        }

        public async Task<List<rptEmpPayBill>> GetEmpPayBill(decimal empId, decimal payId)
        {
            List<rptEmpPayBill> payList= new List<rptEmpPayBill>(); 
            var payListTmp =  await _unitOfWork.ReportsEmployee.GetEmpPayBill(empId, payId);
            var schemeNames = payListTmp.Select(x => x.Scheme_Name).Distinct().ToList();
            if(payListTmp != null && payListTmp.Any()) payList = payListTmp;
            foreach (var schemes in payList.Where(x=> x.Pay_Component_Type == 3))
            {
                schemes.All_Name  = schemes.Scheme_Name;
            }
            foreach (var ledger in payList.Where(x => x.Pay_Component_Type == 5))
            {
                ledger.All_Name = ledger.Led_Name;
            }
            string rsInWords = "";
            if (payListTmp != null)
            {
                if (payListTmp.Count > 0)
                {
                    var lastItem = payList.Last();
                    rsInWords = _utilityHandler.RupeesInWords(lastItem.Pay_Net);

                    foreach (var pay in payList)
                    {
                        pay.RsInWords = rsInWords;
                    }
                }
                else
                {
                    rsInWords = "Zero";
                }
            }
            else
            {
                rsInWords = "Zero";
            }
            if (payListTmp != null) payList = payListTmp;
            return payList;
        }

        public async Task<(List<rptEmpPF> pfList, string rateList)> GetEmpPFLedger(decimal empId, DateTime fromDate, DateTime toDate,string brCode)
        {
            return await _unitOfWork.ReportsEmployee.GetEmpPFLedger(empId, fromDate, toDate, brCode);  
        }

        public async Task<List<rptLoanLedger>> GetStaffLoanLedger(List<decimal> loanIdList, DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsLoan.GetLoanLedger(loanIdList, fromDate, toDate, brCode);
        }

        public async Task<List<RateOfInterestVM>> GetPFRoi(DateTime fromDate, DateTime toDate,string brCode)
        {
            return await _unitOfWork.ReportsEmployee.GetPFRoi (fromDate, toDate, brCode);
        }
    }
}
