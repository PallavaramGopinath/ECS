
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics.Tracing;
    using System.Numerics;

    public partial class Users
    {
        [Key]
        public decimal Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        [Column(TypeName = "bytea")]
        public byte[] Password_Hash { get; set; }
        [Column(TypeName = "bytea")]
        public byte[] Password_Salt { get; set; }
        public string? First_Name { get; set; }
        public string? Last_Name { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_Account_Closed { get; set; }
        public DateTime? Account_Closed_Date { get; set; }
        public DateTime? Last_Login_Date { get; set; }
        public int Failed_Login_Attempts { get; set; } = 0;
        public bool Is_Locked { get; set; } = false;
        public DateTime? Account_Locked_Until { get; set; }
        public DateTime  Created_At { get; set; }
        public DateTime Updated_At { get; set; }
        public string? Mobile_Number { get; set; }
        public string? Reset_Token { get; set; }
        public DateTime? Reset_Token_Expires_At { get; set; }
        public string? BrCode { get; set; }
        public string? Role { get; set; }
    }
}
