
namespace Infin8.Coapp.Dto
{
    public class rptDemandHSIS
    {
        public int Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? Scheme_Name { get; set; }
        public double PIArrear { get; set; }
        public double IODArrear { get; set; }
        public double IntArrear { get; set; }
        public double PrlArrear { get; set; }
        public double PICalc_Amt { get; set; }
        public double IODCalc_Amt { get; set; }
        public double IntCalc_Amt { get; set; }
        public double Prl_Dem { get; set; }
        public double PITotal { get; set; }
        public double IODTotal { get; set; }
        public double IntTotal { get; set; }
        public double PrlTotal { get; set; }
    }
}
