
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Ledger_SubGrp
    {
        [Key]
        public int SubGrp_Id { get; set; }
        public string? SubGrp_Name { get; set; }
        public int Grp_Id { get; set; }
        public bool SubGrp_Mapped { get; set; }
        public decimal Usr_Id { get; set; }
        public int SubGrp_SlNo { get; set; }
        public bool SubGrp_Delete { get; set; }
        public string? BrCode { get; set; }
    }
}
