namespace Infin8.Coapp.Dto
{
    public class PaySurrenderLeaveSalaryVM
    {
        public double Pay_Basic { get; set; }
        public double Pay_PP { get; set; }
        public double Pay_GradePay { get; set; }
        public double Pay_DA_Percent { get; set; }
        public int DA_Id { get; set; }
        public int All_Id { get; set; }
        public string? All_Name { get; set; }
        public double Allowance_Amt { get; set; }
    }
}
