
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Ass_All_Ded
    {
        [Key]
        public int Ass_Ad_Id { get; set; }
        public int Ass_Ad_Ass_Id { get; set; }
        public int Ass_Ad_AllDed_Id { get; set; }
        public double Ass_Ad_AllDed_Amt { get; set; }
        public bool Ass_Ad_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
