namespace Infin8.Coapp.Dto
{
    public class FactoryRecoveryRefundLoanVM
    {
        public int Trn_Id { get; set; }
        public int Loan_Id { get; set; }
        public int Demand_Id { get; set; }
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public int PrlLed_Id { get; set; }
        public int IntLed_Id { get; set; }
        public int PILed_Id { get; set; }
        public string? Loan_No { get; set; }
        public int Mem_Id { get; set; }
        public double PrlColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PIColl_Amt { get; set; }

        public double PrlRefund_Amt { get; set; }
        public double IntRefund_Amt { get; set; }
        public double PIRefund_Amt { get; set; }
    }
}
