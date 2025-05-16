using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Loan_Schemes_Group
    {
        [Key]
        public int Grp_Id { get; set; }
        public string? Grp_Name { get; set; }
        public int Loan_Type { get; set; }
    }
}
