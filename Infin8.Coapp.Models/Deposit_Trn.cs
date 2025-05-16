namespace Infin8.Coapp.Models
{
    using System;

    public partial class Deposit_Trn
    {
        public int DD_Id { get; set; }
        public int DM_Id { get; set; }
        public int TD_Id { get; set; }
        public int Mem_Id { get; set; }
        public int Demand_Id { get; set; }
        public string? DDStatus { get; set; }
        public Nullable<System.DateTime> DDTrn_Date { get; set; }
        public Nullable<System.DateTime> DDDue_Date { get; set; }
        public double Demand_Amt { get; set; }
        public double Receipt_Amt { get; set; }
        public double Paid_Amt { get; set; }
        public double InterestCalculated_Amt { get; set; }
        public Nullable<System.DateTime> InterestCalculated_Date { get; set; }
        public double InterestPaid_Amt { get; set; }
        public double PenalInterestCalculated_Amt { get; set; }
        public Nullable<System.DateTime> PenalInterestCalculated_Date { get; set; }
        public double PenalInterestCollected_Amt { get; set; }
        public bool DD_Oe { get; set; }
        public bool DD_Delete { get; set; }
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public int Trn_SlNo { get; set; }
        public int PbleMaster_Id { get; set; }
        public int Acc_Id { get; set; }
        public double Bal_Amt { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
