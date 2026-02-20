
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Fin_Ledger_Fnl
    {
        [Key]
        public int Fnl_Id { get; set; }
        public string? Fnl_Name { get; set; }
        public decimal Usr_Id { get; set; }
    }
}
