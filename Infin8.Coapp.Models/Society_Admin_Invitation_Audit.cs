using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infin8.Coapp.Models;

[Table("society_admin_invitation_audit")]
public sealed class Society_Admin_Invitation_Audit
{
    [Key]
    public long Audit_Id { get; set; }
    public long Society_Id { get; set; }
    public decimal? User_Id { get; set; }
    public decimal Actor_Id { get; set; }
    public string Event { get; set; } = "";
    public DateTime Occurred_At { get; set; }
    public string? Recipient { get; set; }
}
