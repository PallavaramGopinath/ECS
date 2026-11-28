namespace Infin8.Coapp.Dto;

public sealed class SocietyDirectoryItem
{
    public long SocietyId { get; set; }
    public int SocietyCode { get; set; }
    public string SocietyName { get; set; } = "";
    public string Region { get; set; } = "";
    public string Circle { get; set; } = "";
    public string ProposedCode { get; set; } = "";
    public string? BrCode { get; set; }
    public string? AdminName { get; set; }
    public string? AdminEmail { get; set; }
    public string? AgreementReference { get; set; }
    public DateTime? AgreementDate { get; set; }
    public string Status { get; set; } = "Not prepared";
    public string? LastError { get; set; }
}

public sealed class SocietyOnboardingDetails
{
    public long SocietyId { get; set; }
    public string BrCode { get; set; } = "";
    public string AdminName { get; set; } = "";
    public string AdminEmail { get; set; } = "";
    public string AgreementReference { get; set; } = "";
    public DateTime AgreementDate { get; set; }
}

public sealed class SocietyInvitationReview
{
    public long SocietyId { get; set; }
    public string SocietyName { get; set; } = "";
    public string? BrCode { get; set; }
    public string? AdminEmail { get; set; }
    public string? LoginId { get; set; }
    public string? Problem { get; set; }
    public bool Eligible => Problem is null;
}

public sealed class SocietyInvitationResult
{
    public long SocietyId { get; set; }
    public string Status { get; set; } = "";
    public string? Message { get; set; }
}

public sealed class SocietyDirectoryPage
{
    public int Total { get; set; }
    public List<SocietyDirectoryItem> Items { get; set; } = [];
}
