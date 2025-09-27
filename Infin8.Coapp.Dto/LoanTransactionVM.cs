namespace Infin8.Coapp.Dto
{
    public class LoanTransactionVM
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? Scheme_Name { get; set; }
        public string? Trn_Status { get; set; }
        public DateTime Trn_Date { get; set; }
        public DateTime? Due_Date { get; set; }
        public DateTime? Disb_Date { get; set; }
        public double Disb_Amt { get; set; }
        public double Prl_Sched { get; set; }
        public double Prl_Dem { get; set; }
        public double PICalc_Amt { get; set; }
        public DateTime? PICalc_Date { get; set; }
        public double IODCalc_Amt { get; set; }
        public DateTime? IODCalc_Date { get; set; }
        public double IntCalc_Amt { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public double PIColl_Amt { get; set; }
        public double IODColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PrlColl_Amt { get; set; }
        public double Prl_OB { get; set; }
        public double Prl_OS { get; set; }
        public double Prl_OD { get; set; }
        public double Int_OB { get; set; }
        public double Int_Bal { get; set; }
        public double IOD_Bal { get; set; }
        public double PI_OB { get; set; }
        public double PI_Bal { get; set; }
        public string? Voc_No { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public int? Voc_Type { get; set; }
        public int Trn_SlNo { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
