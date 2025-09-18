using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoLoanDisbursementStaff
    {
        public int AccountId { get; set; }
        public int CashOrAdjustment { get; set; }
        public decimal Employee_Id { get; set; }
        public string? Employee_Name { get; set; }
        public string? Designation { get; set; }
        public DateTime? Transaction_Date { get; set; }
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public int Security_Type { get; set; }
        public string? Loan_No { get; set; }
        public string? ResolutionNo { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public double DisbursementAmount { get; set; }
        public double SanctionedAmount => DisbursementAmount;
        public int Principal_Period { get; set; }
        public int Interest_Period { get; set; }
        public DateTime InstalmentStart_Date { get; set; }
        public double Rate_Of_Interest { get; set; }
        public double InstalmentAmount { get; set; }
        public List<Loan_Repayment_Schedule>? RepaymentSchedule { get; set; }
        public decimal Created_By { get; set; }
        public decimal YrId { get; set; }
        public string? BrCode { get; set; }
        public bool _isError { get; set; }
        public string? errorMessage { get; set; }
    }
}
