
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Ledger_Grp
    {
        [Key]
        public int Grp_Id { get; set; }
        public string? Grp_Name { get; set; }
        public int Fnl_Id { get; set; }
        public bool Grp_Mapped { get; set; }
        public int Usr_Id { get; set; }
        public int Grp_SlNo { get; set; }
        public bool Grp_Delete { get; set; }
        public string? BrCode { get; set; }
    }
}
