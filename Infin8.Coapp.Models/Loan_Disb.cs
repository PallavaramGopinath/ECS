
using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Loan_Disb
    {
        [Key]
        public decimal Disb_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public DateTime Disb_Date { get; set; }
        public int SocDisb_No { get; set; }
        public double SocDisb_Amt { get; set; }
        public Nullable<System.DateTime> SocDisb_Date { get; set; }
        public Nullable<System.DateTime> SocDisbCheque_Date { get; set; }
        public string? SocDisbCheque_No { get; set; }
        public Nullable<System.DateTime> SocEncash_Date { get; set; }
        public double Reim_Amt { get; set; }
        public Nullable<System.DateTime> Reim_Date { get; set; }
        public string? ReimCheque_No { get; set; }
        public Nullable<System.DateTime> ReimCheque_Date { get; set; }
        public bool Disb_Oe { get; set; }
        public int Disb_SlNo { get; set; }
        public bool Is_FinalDisbursement { get; set; }
        public bool LoanDisb_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
