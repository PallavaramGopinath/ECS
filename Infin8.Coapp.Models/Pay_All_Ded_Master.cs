
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_All_Ded_Master
    {
        [Key]
        public decimal All_Id { get; set; }
        public int All_Type { get; set; }
        public string? All_Name { get; set; }
        public decimal Led_Id { get; set; }
        public string? All_Notes { get; set; }
        public bool All_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public bool SLSField { get; set; }
        public string? BrCode { get; set; }
    }
}
