using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoPaySlip
    {
        public decimal Pay_Id { get; set; }
        public int Pay_Month { get; set; }
        public int Pay_Year { get; set; }
        public int TotalDaysInMonth { get; set; }
        public int TotalDays => HeadQuarters + Camp + CasualLeave + EarnedLeave +
                           MedicalLeave + LossOfPay + FestivalHoliday + Holiday;

        public decimal Employee_Id { get; set; }
        public string? Employee_No { get; set; }
        public string? Employee_Name { get; set; }
        public string? Designation { get; set; }
        public decimal DA_Id { get; set; }
        public double DA_Percentage { get; set; }
        public double PF_Percentage { get; set; }

        public int HeadQuarters { get; set; }
        public int Camp { get; set; }
        public int CasualLeave { get; set; }
        public int EarnedLeave { get; set; }
        public int MedicalLeave { get; set; }
        public int LossOfPay { get; set; }
        public int FestivalHoliday { get; set; }
        public int Holiday { get; set; }
        
        public List<DtoPayComponentAssignments>? ComponentAssignments { get; set; }
        public List<PayLoanBalanceVM>? LoanList { get; set; }
        public List<MemberTransactionVM>? SuspeneDueToList { get; set; }
        public double GrossPay { get; set; }
        public double TotalDeductions { get; set; }
        public double NetPay { get; set; }
        public string? BrCode { get; set; }
        public string? PayDescription { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsError { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
    }
}
