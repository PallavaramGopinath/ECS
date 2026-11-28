using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infin8.Coapp.Models;

[Table("society_admin_onboarding")]
public sealed class Society_Admin_Onboarding
{
    [Key]
    public long Society_Id { get; set; }
    public string BrCode { get; set; } = "";
    public string Admin_Name { get; set; } = "";
    public string Admin_Email { get; set; } = "";
    public string Agreement_Reference { get; set; } = "";
    public DateTime Agreement_Date { get; set; }
    public DateTime Email_Verified_At { get; set; }
    public decimal? User_Id { get; set; }
    public string Status { get; set; } = "Ready";
    public DateTime? Last_Attempt_At { get; set; }
    public DateTime? Sent_At { get; set; }
    public string? Last_Error { get; set; }
    public decimal Updated_By { get; set; }
}
