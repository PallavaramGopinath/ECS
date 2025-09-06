using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IPaySlipHandler
    {
        Task<bool> AddPaySlipAsync(Pay_Slip paySlip);
        Task<bool> EditPaySlipAsync(Pay_Slip paySlip);
        Task<DtoPaySlip> DeletePaySlip(DtoPaySlip paySlip);
        Task<List<DtoEmployeeLastPayInfo>> GetEmployeeLastPayInfo();
        Task<List<DtoPayComponentAssignments>> GetPayComponentAssignmentsByEmployeeId(decimal empId, string brCode);
        Task<bool> Find_PaySlipInit(int payMonth, int payYear, string payDes, string brCode);
        Task<bool> IsPreviousPaySlipInitialised(int payMonth, int payYear, string payDes, string brCode);
        Task<bool> IsPaySlipGenerated(decimal payId, decimal empId, string brCode);
        Task<DtoPaySlip> CalculatePaySlip(DtoPaySlip paySlip);
        Task<DtoPaySlip> GeneratePaySlip(DtoPaySlip paySlip);
        Task<decimal> GetPaySlipId(int payMonth, int payYear, string payDes, string brCode);
        Task<DtoPaySlip> GetPaySlipById(decimal payId, decimal memId, string brCode);
        Task<List<DropdownItem>> GetPaySlipListForSalaryPayment(string payDescription, string brCode);
        Task<List<DropdownItem>> GetEmploeeNamesForSalaryPayment(decimal payId, string brCode);
        Task<DropdownItem> GetEmploeeNameForSalaryPayment(decimal empId, decimal payId, string brCode);
        Task<List<Pay_Slip>> GetPaySlipForPayment(List<decimal> empIdList, decimal payId, string brCode);
    }
}
