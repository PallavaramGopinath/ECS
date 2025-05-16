
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Att
    {
        [Key]
        public decimal Att_Id { get; set; }
        public decimal Pay_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public int Att_HQ { get; set; }
        public int Att_CAMP { get; set; }
        public int Att_CL { get; set; }
        public int Att_EL { get; set; }
        public int Att_ML { get; set; }
        public int Att_FH { get; set; }
        public int Att_HD { get; set; }
        public int Att_LOP { get; set; }
        public int Att_TD { get; set; }
        public bool Att_Delete { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
