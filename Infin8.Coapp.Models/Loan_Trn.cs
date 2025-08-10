using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;
    public partial class Loan_Trn
    {
        [Key]
        public decimal Trn_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public decimal Demand_Id { get; set; }
        public string? Trn_Status { get; set; }
        public DateTime Trn_Date { get; set; }
        public Nullable<System.DateTime> Due_Date { get; set; }
        public Nullable<System.DateTime> Disb_Date { get; set; }
        public double Disb_Amt { get; set; }
        public double Prl_Sched { get; set; }
        public double Prl_Dem { get; set; }
        public double PICalc_Amt { get; set; }
        public Nullable<System.DateTime> PICalc_Date { get; set; }
        public double IODCalc_Amt { get; set; }
        public Nullable<System.DateTime> IODCalc_Date { get; set; }
        public double IntCalc_Amt { get; set; }
        public Nullable<System.DateTime> IntCalc_Date { get; set; }
        public double PIColl_Amt { get; set; }
        public double IODColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PrlColl_Amt { get; set; }
        public double PrlReim_Schedule { get; set; }
        public double PrlReim_Demand { get; set; }
        public double PIReimCalc_Amt { get; set; }
        public Nullable<System.DateTime> PIReimCalc_Date { get; set; }
        public double IODReimCalc_Amt { get; set; }
        public Nullable<System.DateTime> IODReimCalc_Date { get; set; }
        public double IntReimCalc_Amt { get; set; }
        public Nullable<System.DateTime> IntReimCalc_Date { get; set; }
        public double Soc_Roi { get; set; }
        public double SocPI_Rate { get; set; }
        public double Reim_Roi { get; set; }
        public double ReimPi_Rate { get; set; }
        public bool TrnTr_Oe { get; set; }
        public bool TrnTr_Delete { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public int Trn_SlNo { get; set; }
        public bool ExpiredDeletion { get; set; }
        //public double Prl_OS { get; set; }
        //public double Prl_OD { get; set; }
        //public double Int_Bal { get; set; }
        //public double PI_Bal { get; set; }
        //public double IOD_Bal { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; } = "V";
    }
}
