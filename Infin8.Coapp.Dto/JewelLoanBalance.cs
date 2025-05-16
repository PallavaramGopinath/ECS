
namespace Infin8.Coapp.Dto
{
    public class JewelLoanBalance
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public double San_Amt { get; set; }
        public DateTime San_Date { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public DateTime JL_DueDate { get; set; }
        public double Prl_Bal { get; set; }
        public double Int_Bal { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public double PI_Bal { get; set; }
        public DateTime? PICalc_Date { get; set; }
        public double IntCalc_Amt { get; set; }
        public double PICalc_Amt { get; set; }
        public DateTime? IntCalc_DateNow { get; set; }
        public DateTime? PICalc_DateNow { get; set; }
        public string? Scheme_Name { get; set; }
        public decimal PrlLed_Id { get; set; }
        public decimal IntLed_Id { get; set; }
        public decimal PILed_Id { get; set; }
        public int Inst_Type { get; set; }
        public double Prl_OD { get; set; }
        public double Int_OD { get; set; }
        public DateTime? Due_Date { get; set; }
        public double Inst_Amt { get; set; }
        public DateTime? FirstInt_DueDate { get; set; }
        public DateTime? FirstPrl_DueDate { get; set; }
        public double Prl_Demand { get; set; }
        public double Applied_roi { get; set; }
        public double Applied_pi { get; set; }
        public DateTime? Demand_Date { get; set; }
        public double IntDue_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public int PI_Application { get; set; }
        public string? Status { get; set; }
        public double Recovery_Amount { get; set; }
    }
}
