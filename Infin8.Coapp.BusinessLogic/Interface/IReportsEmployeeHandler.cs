using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IReportsEmployeeHandler
    {
        Task<List<rptEmpPayBill>> GetEmpPayBill(decimal empId, decimal payId);
        Task<(List<rptEmpPF> pfList, string rateList)> GetEmpPFLedger(decimal empId, DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptEmp12MonthsSalary>> GetEmployee12MonthsSalary(DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptLoanLedger>> GetStaffLoanLedger(List<decimal> loanIdList, DateTime fromDate, DateTime toDate,string brCode);
        Task<List<RateOfInterestVM>> GetPFRoi(DateTime fromDate, DateTime toDate,string brCode);
    }
}
