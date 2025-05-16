namespace Infin8.Coapp.Dto
{
    public class rptLoanDemand
    {
        public decimal Loan_Id { get; set; }
        public DateTime Trn_Date { get; set; }
        public DateTime? Due_Date { get; set; }
        public double Prl_Sched { get; set; }
        public double PrlColl_Amt { get; set; }
        public double Prl_Bal { get; set; }
        public double Prl_OS { get; set; }
        public double Prl_OD { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public double IntCalc_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double Int_Bal { get; set; }
    }
}
