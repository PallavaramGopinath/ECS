
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_Salary
    {
        [Key]
        public int MemberSalary_Id { get; set; }
        public int LoanElig_Id { get; set; }
        public int MST_Id { get; set; }
        public double Amount { get; set; }
        public bool MemberSalary_Delete { get; set; }
        public int Voc_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public double BasicPay { get; set; }
        public double GrossPay { get; set; }
        public double SocietyDeduction { get; set; }
        public double OtherDeductions { get; set; }
        public double TotalDeductions { get; set; }
        public double NetPay { get; set; }
        public string? BrCode { get; set; }
    }
}
