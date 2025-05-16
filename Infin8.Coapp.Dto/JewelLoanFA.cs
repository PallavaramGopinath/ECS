
namespace Infin8.Coapp.Dto
{
    public class JewelLoanFA
    {
        public int Loan_Id { get; set; }
        public int Loan_Type { get; set; }
        public DateTime San_Date { get; set; }
        public int Prl_Prd { get; set; }
        public int Roi { get; set; }
        public int Pi { get; set; }
        public double Disb_Amt { get; set; }
        public double PrlColl_Amt { get; set; }
        public double IntCalc_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PICalc_Amt { get; set; }
        public double PIColl_Amt { get; set; }
        public DateTime? MaxIntCalc_Date { get; set; }
        public DateTime? MaxPICalc_Date { get; set; }
    }
}
