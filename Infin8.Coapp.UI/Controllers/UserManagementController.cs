using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.API.Controllers;

[ApiController]
[Route("api/user-management")]
[Authorize(Roles = "Admin,PlatformOnboarding")]
public sealed class UserManagementController(CSISContext context, IUserHandler userHandler) : ControllerBase
{
    [HttpGet("branches")]
    public async Task<ActionResult<List<ManagedBranchDto>>> Branches()
    {
        var actor = await ActorAsync();
        if (actor is null) return Unauthorized();
        if (!User.IsInRole("PlatformOnboarding"))
            return Ok(new List<ManagedBranchDto> { new() { BrCode = actor.BrCode!, Name = actor.BrCode! } });
        var branches = await (from onboarding in context.Society_Admin_Onboarding.AsNoTracking()
            join bank in context.Gen_Bank_Name.AsNoTracking() on onboarding.BrCode equals bank.BrCode
            where onboarding.Status == "Sent" && bank.Bank_Type == 3
            orderby bank.Bank_Name
            select new ManagedBranchDto { BrCode = onboarding.BrCode, Name = bank.Bank_Name ?? onboarding.BrCode })
            .ToListAsync();
        if (!branches.Any(x => x.BrCode == actor.BrCode))
            branches.Insert(0, new() { BrCode = actor.BrCode!, Name = $"Platform ({actor.BrCode})" });
        return Ok(branches);
    }

    [HttpGet]
    public async Task<ActionResult<List<ManagedUserDto>>> List([FromQuery] string? brCode)
    {
        var actor = await ActorAsync();
        if (actor is null) return Unauthorized();
        var branch = await ResolveBranchAsync(brCode, actor);
        if (branch is null) return Forbid();
        var platform = User.IsInRole("PlatformOnboarding");
        var ownBranch = branch == actor.BrCode;
        var rows = await context.Users.AsNoTracking()
            .Where(x => x.BrCode == branch &&
                (x.Role == "PlatformOnboarding" ? platform && ownBranch
                    : x.Role == null || !x.Role.Contains("PlatformOnboarding")))
            .OrderBy(x => x.Username).ToListAsync();
        return Ok(rows.Select(Map).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ManagedUserDto>> Create([FromBody] ManagedUserRequest request)
    {
        if (!SameOrigin()) return Forbid();
        var actor = await ActorAsync();
        if (actor is null) return Unauthorized();
        if (request is null) return BadRequest("Enter user details.");
        var branch = await ResolveBranchAsync(request.BrCode, actor);
        if (branch is null) return Forbid();
        var problem = Validate(request, creating: true);
        if (problem is not null) return BadRequest(problem);
        if (request.Role == "PlatformOnboarding" && branch != actor.BrCode)
            return Forbid();
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await context.Users.AnyAsync(x => x.Username != null && x.Username.ToLower() == username.ToLower()
            || x.Email != null && x.Email.ToLower() == email))
            return Conflict("Username or email is already in use.");

        var now = DateTime.UtcNow;
        userHandler.CreatePasswordHash(request.TemporaryPassword!, out var hash, out var salt);
        var user = new Users
        {
            Id = (await context.Users.MaxAsync(x => (decimal?)x.Id) ?? 11010000000) + 1,
            Username = username, Email = email, First_Name = request.FirstName.Trim(),
            Last_Name = request.LastName.Trim(), Mobile_Number = request.MobileNumber?.Trim(),
            Role = request.Role, BrCode = branch, Password_Hash = hash, Password_Salt = salt,
            Is_Active = true, Created_At = now, Updated_At = now,
            Must_Change_Password = true, Invitation_Expires_At = now.AddDays(2),
            Security_Stamp = Guid.NewGuid().ToString("N")
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return Created($"api/user-management/{user.Id}", Map(user));
    }

    [HttpPut("{id:decimal}")]
    public async Task<ActionResult<ManagedUserDto>> Update(decimal id, [FromBody] ManagedUserRequest request)
    {
        if (!SameOrigin()) return Forbid();
        var actor = await ActorAsync();
        if (actor is null) return Unauthorized();
        if (id == actor.Id) return BadRequest("You cannot change your own account here.");
        var user = await ManagedUserAsync(id, actor);
        if (user is null) return NotFound();
        var problem = Validate(request, creating: false);
        if (problem is not null) return BadRequest(problem);
        if (request.Role == "PlatformOnboarding" && user.BrCode != actor.BrCode)
            return Forbid();
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await context.Users.AnyAsync(x => x.Id != id &&
            (x.Username != null && x.Username.ToLower() == username.ToLower()
            || x.Email != null && x.Email.ToLower() == email)))
            return Conflict("Username or email is already in use.");
        if ((!request.IsActive || request.Role != "Admin") && user.Role == "Admin" &&
            !await context.Users.AnyAsync(x => x.BrCode == user.BrCode && x.Id != id
                && x.Role == "Admin" && x.Is_Active && !x.Is_Account_Closed))
            return Conflict("The society must retain an active administrator.");

        user.Username = username;
        user.Email = email;
        user.First_Name = request.FirstName.Trim();
        user.Last_Name = request.LastName.Trim();
        user.Mobile_Number = request.MobileNumber?.Trim();
        user.Role = request.Role;
        user.Is_Active = request.IsActive;
        user.Is_Account_Closed = false;
        user.Account_Closed_Date = null;
        if (!string.IsNullOrWhiteSpace(request.TemporaryPassword))
        {
            userHandler.CreatePasswordHash(request.TemporaryPassword, out var hash, out var salt);
            user.Password_Hash = hash;
            user.Password_Salt = salt;
            user.Must_Change_Password = true;
            user.Invitation_Expires_At = DateTime.UtcNow.AddDays(2);
        }
        user.Security_Stamp = Guid.NewGuid().ToString("N");
        user.Updated_At = DateTime.UtcNow;
        context.RefreshTokens.RemoveRange(context.RefreshTokens.Where(x => x.User_Id == id));
        await context.SaveChangesAsync();
        return Ok(Map(user));
    }

    [HttpDelete("{id:decimal}")]
    public async Task<IActionResult> Delete(decimal id)
    {
        if (!SameOrigin()) return Forbid();
        var actor = await ActorAsync();
        if (actor is null) return Unauthorized();
        if (id == actor.Id) return BadRequest("You cannot delete your own account.");
        var user = await ManagedUserAsync(id, actor);
        if (user is null) return NotFound();
        if (user.Role == "Admin" && !await context.Users.AnyAsync(x => x.BrCode == user.BrCode
            && x.Id != id && x.Role == "Admin" && x.Is_Active && !x.Is_Account_Closed))
            return Conflict("The society must retain an active administrator.");
        user.Is_Active = false;
        user.Is_Account_Closed = true;
        user.Account_Closed_Date = DateTime.UtcNow;
        user.Updated_At = DateTime.UtcNow;
        user.Security_Stamp = Guid.NewGuid().ToString("N");
        context.RefreshTokens.RemoveRange(context.RefreshTokens.Where(x => x.User_Id == id));
        await context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<Users?> ActorAsync()
    {
        if (!decimal.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return null;
        return await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Is_Active
            && !x.Is_Account_Closed && x.BrCode != null);
    }

    private async Task<Users?> ManagedUserAsync(decimal id, Users actor)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null || await ResolveBranchAsync(user.BrCode, actor) is null) return null;
        if ((user.Role ?? "").Contains("PlatformOnboarding") &&
            (!User.IsInRole("PlatformOnboarding") || user.BrCode != actor.BrCode
                || user.Role != "PlatformOnboarding")) return null;
        return user;
    }

    private async Task<string?> ResolveBranchAsync(string? requested, Users actor)
    {
        if (!User.IsInRole("PlatformOnboarding"))
            return requested is null || requested == actor.BrCode ? actor.BrCode : null;
        var branch = string.IsNullOrWhiteSpace(requested) ? actor.BrCode : requested.Trim();
        if (branch == actor.BrCode) return branch;
        return await context.Society_Admin_Onboarding.AsNoTracking()
            .AnyAsync(x => x.BrCode == branch && x.Status == "Sent") ? branch : null;
    }

    private string? Validate(ManagedUserRequest request, bool creating)
    {
        var allowed = User.IsInRole("PlatformOnboarding")
            ? new[] { "PlatformOnboarding", "Admin", "Maker", "Checker" }
            : new[] { "Admin", "Maker", "Checker" };
        if (request is null || !allowed.Contains(request.Role)) return "Select an allowed role.";
        if (string.IsNullOrWhiteSpace(request.Username) || !Utilities.IsValidUsername(request.Username.Trim()))
            return "Username must have 5 to 30 letters, digits, dots, dashes or underscores.";
        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email)
            || request.Email.Length > 254) return "Enter a valid email address.";
        if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Length > 50
            || string.IsNullOrWhiteSpace(request.LastName) || request.LastName.Length > 50)
            return "Enter first and last names of at most 50 characters each.";
        if (!string.IsNullOrWhiteSpace(request.MobileNumber)
            && (request.MobileNumber.Length != 10 || !request.MobileNumber.All(char.IsDigit)))
            return "Mobile number must contain 10 digits.";
        if (creating || !string.IsNullOrWhiteSpace(request.TemporaryPassword))
        {
            var password = request.TemporaryPassword;
            if (password is null || password.Length < 12 || !password.Any(char.IsUpper)
                || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
                return "Temporary password must have at least 12 characters, including upper and lower case letters and a digit.";
        }
        return null;
    }

    private bool SameOrigin()
    {
        var origin = Request.Headers.Origin.ToString();
        return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && uri.Scheme == Request.Scheme && uri.Authority == Request.Host.Value;
    }

    private static ManagedUserDto Map(Users user) => new()
    {
        Id = user.Id, Username = user.Username ?? "", Email = user.Email ?? "",
        FirstName = user.First_Name ?? "", LastName = user.Last_Name ?? "",
        MobileNumber = user.Mobile_Number, Role = user.Role ?? "", BrCode = user.BrCode ?? "",
        IsActive = user.Is_Active && !user.Is_Account_Closed,
        MustChangePassword = user.Must_Change_Password
    };
}
