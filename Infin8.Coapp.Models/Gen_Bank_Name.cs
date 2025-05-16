
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Gen_Bank_Name
    {
        [Key]
        public decimal Bank_Id { get; set; }
        public string? Bank_Name { get; set; }
        public string? Bank_Address { get; set; }
        public string? Bank_City { get; set; }
        public string? Bank_Phoneno { get; set; }
        public string? Bank_Address2 { get; set; }
        public string? Bank_Fax { get; set; }
        public string? Bank_Email { get; set; }
        public string? Bank_Pincode { get; set; }
        public string? Bank_ShortName { get; set; }
        public int Bank_Type { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
