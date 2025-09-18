using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoLoanDisbursementPLDB
    {
        public int AccountId { get; set; }
        public int CashOrAdjustment { get; set; }
        public decimal Mem_Id { get; set; }
        public string? Member_No { get; set; }
        public string? Member_Name { get; set; }
        public DateTime Tranaction_Date { get; set; }
        public decimal Applicant_Id { get; set; }
        public string? Applicant_No { get; set; }
        public string? Applicant_Name { get; set; }
        public int Applicant_Status { get; set; }
        public int MyProperty { get; set; }
        public int Scheme_Id { get; set; }
        public int Security_Type { get; set; }
        public string? Loan_No { get; set; }

        //public string? LANo { get; set; }
        //public string? ResolutionNo { get; set; }
        //public DateTime? ResolutionDate { get; set; }
        //public double SanctionedAmount { get; set; }
        //public double DisbursementAmount { get; set; }
        //public int InstalmentDay { get; set; }
        //public int GracePeriod { get; set; }
        //public int Period_Of_Loan { get; set; }
        //public DateTime? FirstInsterestDueDate { get; set; }
        //public DateTime? FirstPrincipalDueDate { get; set; }
        //public double Rate_Of_Interest { get; set; }
        //public double Penal_Rate { get; set; }
        //public double InstalmentAmount { get; set; }
        //public DateTime? MortgageBondDate { get; set; }
        //public string? MortgageBondNo { get; set; }
        //public DateTime? MortgageBondRegistrationNo { get; set; }
        //public string? SARDBLoanNo { get; set; }
        //public string? DVNo { get; set; }
        //public string? SubRegistrarOfficeName { get; set; }

        public List<DtoLoanRepaymentSchedule>? RepaymentSchedule{ get; set; }
        public DtoLoanDisbursementLT? LTLoanDisbursement  { get; set; }

        //public List<Loan_Members>? LoanMembersList { get; set; }

        //public List<Loan_Repayment_Schedule>?  RepaymentSchedule { get; set; }
        public DtoJewelLoanDisbursement? JewelLoanDisbursement { get; set; }
        public decimal Yr_Id { get; set; }
        public decimal Created_By { get; set; }
        public string? BrCode { get; set; }
       
    }
}
