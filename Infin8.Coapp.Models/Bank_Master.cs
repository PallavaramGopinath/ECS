using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    public class Bank_Master
    {
        [Key]
        public int Bank_Id { get; set; }
        public string? Bank_Name { get; set; }
        public string? Bank_ShortName { get; set; }
        public bool Bank_Delete { get; set; }
        public int? Usr_Id { get; set; }
        public int? Yr_Id { get; set; }
    }
}
