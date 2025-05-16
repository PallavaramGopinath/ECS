
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_PassBookStatement
    {
        [Key]
        public int PB_Id { get; set; }
        public int Mem_Id { get; set; }
        public int Loan_Id { get; set; }
        public int DM_Id { get; set; }
        public int Led_Id { get; set; }
        public int TD_Id { get; set; }
        public Nullable<System.DateTime> PB_Date { get; set; }
        public string? PB_Status { get; set; }
        public string? PB_No { get; set; }
        public string? PB_TrnNo { get; set; }
        public string? PB_AccountName { get; set; }
        public int Prl_OB { get; set; }
        public int Prl_Rpt { get; set; }
        public int Prl_Pmt { get; set; }
        public int Prl_CB { get; set; }
        public int Prl_OD { get; set; }
        public int Int_OB { get; set; }
        public int Int_Calc { get; set; }
        public int Int_Rpt { get; set; }
        public int Int_Pmt { get; set; }
        public int Int_CB { get; set; }
        public int Int_OD { get; set; }
        public string? MonthYear { get; set; }
        public string? MonthYearString { get; set; }
        public string? BrCode { get; set; }
    }
}
