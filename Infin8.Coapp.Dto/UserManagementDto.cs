namespace Infin8.Coapp.Dto;

public sealed class ManagedUserDto
{
    public decimal Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? MobileNumber { get; set; }
    public string Role { get; set; } = "";
    public string BrCode { get; set; } = "";
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
}

public sealed class ManagedUserRequest
{
    public string? BrCode { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? MobileNumber { get; set; }
    public string Role { get; set; } = "";
    public string? TemporaryPassword { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ManagedBranchDto
{
    public string BrCode { get; set; } = "";
    public string Name { get; set; } = "";
}
