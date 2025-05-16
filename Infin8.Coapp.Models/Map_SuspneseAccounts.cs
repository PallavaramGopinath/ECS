
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Map_SuspenseAccounts

    {
        [Key]
        public decimal Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public int Sus_Type { get; set; }
        public int IntCalc_Application { get; set; }
        public decimal IntLed_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
