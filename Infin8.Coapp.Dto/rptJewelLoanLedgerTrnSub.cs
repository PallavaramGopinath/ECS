namespace Infin8.Coapp.Dto
{
    public class rptJewelLoanLedgerTrnSub
    {
        public int Loan_Id { get; set; }
        public DateTime San_Date { get; set; }
        public double San_Amt { get; set; }
        public DateTime Trn_Date { get; set; }
        public double Disb_Amt { get; set; }
        public double PICalc_Amt { get; set; }
        public DateTime? PICalc_Date { get; set; }
        public double IntCalc_Amt { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public double PIColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PrlColl_Amt { get; set; }
        public double Prl_OS { get; set; }
        public double Int_Bal { get; set; }
        public double PI_Bal { get; set; }
    }
}
