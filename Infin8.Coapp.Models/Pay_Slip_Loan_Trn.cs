
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Slip_Loan_Trn
    {
        [Key]
        public decimal Tr_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public decimal Pay_Id { get; set; }
        public Nullable<System.DateTime> Rpt_Date { get; set; }
        public double Amt_Coll { get; set; }
        public double Prl_Schedule { get; set; }
        public double Prl_Coll { get; set; }
        public Nullable<System.DateTime> Int_Calc_Upto { get; set; }
        public double Int_Calc_Amt { get; set; }
        public double Int_Coll { get; set; }
        public bool LoanTr_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
