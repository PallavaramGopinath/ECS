
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class TermDeposit_Trn
    {
        [Key]
        public decimal TDTrn_Id { get; set; }
        public DateTime  Trn_Date { get; set; }
        public decimal TD_Id { get; set; }
        public double DepositDemandAmount { get; set; }
        public double DepositReceiptAmount { get; set; }
        public double MaturityAmount { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public Nullable<System.DateTime> InterestAppliedDate { get; set; }
        public double InterestPaidAmount { get; set; }
        public double DepositPaidAmount { get; set; }
        public double PenalCalculatedAmount { get; set; }
        public Nullable<System.DateTime> PenalAppliedDate { get; set; }
        public double PenalReceivedAmount { get; set; }
        public int NoOfInstalments { get; set; }
        public int InstalmentAmount { get; set; }
        public Nullable<System.DateTime> LastInstalmentDate { get; set; }
        public Nullable<System.DateTime> LastPaymentDate { get; set; }
        public bool TD_OE { get; set; }
        public bool TD_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public int Trn_SlNo { get; set; }
        public decimal Demand_Id { get; set; }
        public decimal TDCalc_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
