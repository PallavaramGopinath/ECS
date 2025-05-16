namespace Infin8.Coapp.Dto
{
    public class rptJewelLoanStockRegisterList
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? PmtLoan_No { get; set; }
        public DateTime Trn_Date { get; set; }
        public double Disb_Amt { get; set; }
        public double GrossWeight { get; set; }
        public double NetValue { get; set; }
        public int Trn_SlNo { get; set; }
        public double Rpt_GrossWeight { get; set; }
        public double Rpt_Wastage { get; set; }
        public double Rpt_NetWeight { get; set; }
        public double Rpt_NetValue { get; set; }
        public string? RptLoan_No { get; set; }
        public double PrlColl_Amt { get; set; }
        public double Pmt_GrossWeight { get; set; }
        public double Pmt_Wastage { get; set; }
        public double Pmt_NetWeight { get; set; }
        public double Pmt_NetValue { get; set; }
        public int NoOfBags { get; set; }
        public int BalNoOfBags { get; set; }
        public double TotalGrossWeight { get; set; }
        public double TotalNetValue { get; set; }
        public double Loan_OS { get; set; }
        public double RatePerGram { get; set; }
    }
}
