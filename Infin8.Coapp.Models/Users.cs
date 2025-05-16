
namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.Tracing;

    public partial class Users
    {
        [Key]
        public int id { get; set; }
        public string? username { get; set; }
        public string? email { get; set; }
        public byte[]? password_hash { get; set; }
        public byte[]? password_salt { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public bool is_active { get; set; }
        public bool is_account_closed { get; set; }
        public DateTime? account_closed_date { get; set; }
        public DateTime? last_login_date { get; set; }
        public int failed_login_attempts { get; set; } = 0;
        public bool is_locked { get; set; } = false;
        public DateTime? account_locked_until { get; set; }
        public DateTime  created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string? mobile_number { get; set; }
        public string? reset_token { get; set; }
        public DateTime? reset_token_expires_at { get; set; }
        public string? brcode { get; set; }
        public string? role { get; set; }
    }
}
