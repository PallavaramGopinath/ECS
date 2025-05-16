
namespace Infin8.Coapp.Dto
{
    public  class JewelLoanDemand
    {
        public int Loan_Id { get; set; }
        public DateTime San_Date { get; set; }
        public double San_Amt { get; set; }
        public int Prl_Prd { get; set; }
        public double Inst_Amt { get; set; }
        public double IntDue_Amt { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public DateTime? Due_Date { get; set; }
        public DateTime FirstPrl_DueDate { get; set; }
        public double Prl_Dem { get; set; }
        public double Prl_OD { get; set; }
        public double Int_OD { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public double PI_Bal { get; set; }
        public DateTime? PICalc_Date { get; set; }
    }
}
