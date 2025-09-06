namespace Infin8.Coapp.Dto
{
    public class PayLoanBalanceVM
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public double San_Amt { get; set; }
        public DateTime FirstInt_DueDate { get; set; }
        public DateTime FirstPrl_DueDate { get; set; }
        public DateTime San_Date { get; set; }
        public int Prl_Prd { get; set; }
        public int Int_Prd { get; set; }
        public double Inst_Amt { get; set; }
        public double Roi { get; set; }
        public string? Scheme_Name { get; set; }
        public decimal Scheme_Id { get; set; }
        public int Loan_Type { get; set; }
        public decimal PrlLed_Id { get; set; }
        public decimal IntLed_Id { get; set; }
        public int StaffLoan_Int_Type { get; set; }
        public DateTime MaxTrn_Date { get; set; }
        public double SumDisb_Amt { get; set; }
        public double SumPrl_Sched { get; set; }
        public double SumPrl_Dem { get; set; }
        public double SumPrlColl_Amt { get; set; }
        public double SumIntCalc_Amt { get; set; }
        public DateTime? MaxIntCalc_Date { get; set; }
        public double SumIntColl_Amt { get; set; }
        public double PrlOS { get; set; }
        public double PrlOD { get; set; }
        public double IntBal { get; set; }
        public double PrlDemand { get; set; }
        public double IntDemand { get; set; }
        public double IntCalc { get; set; }
        public DateTime? IntCalcDate { get; set; }
        public double TotalRecovery => PrlRecovery + IntRecovery;  ///PrlDemand + PrlOD  + IntDemand;
        public double PrlRecovery { get; set; }
        public double IntRecovery { get; set; }
    }
}
