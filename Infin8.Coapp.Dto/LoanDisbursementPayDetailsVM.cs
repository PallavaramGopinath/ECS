namespace Infin8.Coapp.Dto
{
    public class LoanDisbursementPayDetailsVM
    {
        public double BasicPay { get; set; }
        public double SocietyDeductions { get; set; }
        public double TotalDeductions { get; set; }
        public double NetPay { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfRetirement { get; set; }
    }
}
