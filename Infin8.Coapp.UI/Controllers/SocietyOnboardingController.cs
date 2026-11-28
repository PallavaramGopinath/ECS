using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infin8.Coapp.API.Controllers;

[ApiController]
[Route("api/society-onboarding")]
[Authorize(Roles = "PlatformOnboarding")]
public sealed class SocietyOnboardingController(
    CSISContext context, IUserHandler userHandler, IConfiguration configuration) : ControllerBase
{
    [HttpGet("societies")]
    public async Task<ActionResult<SocietyDirectoryPage>> Societies(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            return BadRequest("Invalid page or page size.");

        var term = $"%{(search ?? "").Trim()}%";
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        await connection.OpenAsync(HttpContext.RequestAborted);
        try
        {
            const string filter = "s.society_name ILIKE @search OR s.proposed_combined_society_code ILIKE @search OR s.region ILIKE @search OR s.circle ILIKE @search OR s.society_code::text ILIKE @search OR o.brcode ILIKE @search";
            await using var count = new NpgsqlCommand($"SELECT count(*) FROM public.societies s LEFT JOIN public.society_admin_onboarding o ON o.society_id=s.society_id WHERE {filter}", connection);
            count.Parameters.AddWithValue("search", term);
            var result = new SocietyDirectoryPage { Total = Convert.ToInt32(await count.ExecuteScalarAsync(HttpContext.RequestAborted)) };
            await using var rows = new NpgsqlCommand($"""
                SELECT s.society_id, s.society_code, s.society_name, s.region, s.circle,
                       s.proposed_combined_society_code, o.brcode, o.admin_name, o.admin_email,
                       o.agreement_reference, o.agreement_date, o.status, o.last_error
                FROM public.societies s
                LEFT JOIN public.society_admin_onboarding o ON o.society_id=s.society_id
                WHERE {filter}
                ORDER BY s.society_name, s.society_id LIMIT @limit OFFSET @offset
                """, connection);
            rows.Parameters.AddWithValue("search", term);
            rows.Parameters.AddWithValue("limit", pageSize);
            rows.Parameters.AddWithValue("offset", (page - 1) * pageSize);
            await using var reader = await rows.ExecuteReaderAsync(HttpContext.RequestAborted);
            while (await reader.ReadAsync(HttpContext.RequestAborted))
                result.Items.Add(new SocietyDirectoryItem
                {
                    SocietyId = reader.GetInt64(0),
                    SocietyCode = reader.GetInt32(1),
                    SocietyName = reader.GetString(2),
                    Region = reader.GetString(3),
                    Circle = reader.GetString(4),
                    ProposedCode = reader.GetString(5),
                    BrCode = reader.IsDBNull(6) ? null : reader.GetString(6),
                    AdminName = reader.IsDBNull(7) ? null : reader.GetString(7),
                    AdminEmail = reader.IsDBNull(8) ? null : reader.GetString(8),
                    AgreementReference = reader.IsDBNull(9) ? null : reader.GetString(9),
                    AgreementDate = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                    Status = reader.IsDBNull(11) ? "Not prepared" : reader.GetString(11),
                    LastError = reader.IsDBNull(12) ? null : reader.GetString(12)
                });
            return Ok(result);
        }
        finally { await connection.CloseAsync(); }
    }

    [HttpPut("details/{societyId:long}")]
    public async Task<IActionResult> SaveDetails(long societyId, [FromBody] SocietyOnboardingDetails details)
    {
        if (!SameOrigin()) return Forbid();
        if (details.SocietyId != societyId || string.IsNullOrWhiteSpace(details.BrCode)
            || string.IsNullOrWhiteSpace(details.AdminName)
            || string.IsNullOrWhiteSpace(details.AgreementReference)
            || !new EmailAddressAttribute().IsValid(details.AdminEmail)
            || details.AgreementDate.Date > DateTime.UtcNow.Date
            || details.AgreementDate == default)
            return BadRequest("Provide the branch, signed agreement reference and date, administrator name, and verified email.");

        var exists = await SocietyNameAsync(societyId);
        if (exists is null) return NotFound();
        var brCode = details.BrCode.Trim();
        if (await context.Gen_Bank_Name.CountAsync(x => x.BrCode == brCode && x.Bank_Type == 3) != 1)
            return BadRequest("The branch code must identify exactly one ECS society.");
        var conflict = await context.Society_Admin_Onboarding.AnyAsync(x => x.BrCode == brCode && x.Society_Id != societyId);
        if (conflict) return Conflict("This branch is already mapped to another society.");

        var row = await context.Society_Admin_Onboarding.FindAsync(societyId);
        if (row is not null && row.Status is "Sending" or "Sent")
            return Conflict("An invitation is in progress or already sent. Contact support to change its recipient.");
        row ??= new Society_Admin_Onboarding { Society_Id = societyId };
        if (context.Entry(row).State == EntityState.Detached) context.Society_Admin_Onboarding.Add(row);
        row.BrCode = brCode;
        row.Admin_Name = details.AdminName.Trim();
        row.Admin_Email = details.AdminEmail.Trim().ToLowerInvariant();
        row.Agreement_Reference = details.AgreementReference.Trim();
        row.Agreement_Date = details.AgreementDate.Date;
        row.Email_Verified_At = DateTime.UtcNow;
        row.Status = "Ready";
        row.Updated_By = OperatorId();
        context.Society_Admin_Invitation_Audit.Add(new()
        {
            Society_Id = societyId, Actor_Id = OperatorId(), Event = "ContactVerified",
            Occurred_At = DateTime.UtcNow, Recipient = row.Admin_Email
        });
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("review")]
    public async Task<ActionResult<List<SocietyInvitationReview>>> Review([FromBody] long[] societyIds)
    {
        if (!SameOrigin()) return Forbid();
        if (societyIds.Length is < 1 or > 10 || societyIds.Distinct().Count() != societyIds.Length)
            return BadRequest("Select 1 to 10 distinct societies.");
        var results = new List<SocietyInvitationReview>();
        foreach (var id in societyIds)
            results.Add(await ReviewOneAsync(id));
        return Ok(results);
    }

    [HttpPost("send")]
    public async Task<ActionResult<List<SocietyInvitationResult>>> Send([FromBody] long[] societyIds)
    {
        if (!SameOrigin()) return Forbid();
        if (societyIds.Length is < 1 or > 10 || societyIds.Distinct().Count() != societyIds.Length)
            return BadRequest("Select 1 to 10 distinct societies.");
        if (!MailConfigured()) return StatusCode(503, "Mail settings are not configured.");
        var results = new List<SocietyInvitationResult>();
        foreach (var id in societyIds)
            results.Add(await SendOneAsync(id));
        return Ok(results);
    }

    private async Task<SocietyInvitationReview> ReviewOneAsync(long id)
    {
        var name = await SocietyNameAsync(id);
        var row = await context.Society_Admin_Onboarding.AsNoTracking().FirstOrDefaultAsync(x => x.Society_Id == id);
        var result = new SocietyInvitationReview
        {
            SocietyId = id, SocietyName = name ?? $"Society {id}",
            BrCode = row?.BrCode, AdminEmail = row?.Admin_Email,
            LoginId = row is null ? null : $"ecs-{id}"
        };
        if (name is null) result.Problem = "Society was not found.";
        else if (row is null) result.Problem = "Add agreement, branch mapping, and administrator contact.";
        else if (row.Status == "Sent" && !await context.Users.AnyAsync(x => x.Id == row.User_Id
            && x.Must_Change_Password && x.Invitation_Expires_At <= DateTime.UtcNow))
            result.Problem = "Invitation already sent.";
        else if (row.Status == "Sending") result.Problem = "A send is already in progress.";
        else if (row.Agreement_Date == default || string.IsNullOrWhiteSpace(row.Agreement_Reference))
            result.Problem = "Agreement is missing.";
        else if (string.IsNullOrWhiteSpace(row.Admin_Name) || !new EmailAddressAttribute().IsValid(row.Admin_Email))
            result.Problem = "Administrator name or email is invalid.";
        else if (await context.Gen_Bank_Name.CountAsync(x => x.BrCode == row.BrCode && x.Bank_Type == 3) != 1)
            result.Problem = "ECS branch mapping is invalid.";
        else if (!await context.Fin_Yr_Master.AnyAsync(x => x.BrCode == row.BrCode && !x.Yr_closed && x.From_Date != null && x.To_Date != null)
            || !await context.Business_Day.AnyAsync(x => x.BrCode == row.BrCode && x.Calendar_Status == "N" && !x.Calendar_Delete))
            result.Problem = "Branch financial year or business day is not ready.";
        else if (!await context.mem_master.AnyAsync(x => x.brcode == row.BrCode))
            result.Problem = "No society data is loaded for this branch yet.";
        else if (await context.Users.AnyAsync(x => x.BrCode == row.BrCode && x.Role == "Admin" && x.Is_Active && x.Id != row.User_Id))
            result.Problem = "The branch already has an active administrator.";
        else if (await context.Users.AnyAsync(x => x.Username == result.LoginId && x.Id != row.User_Id))
            result.Problem = "The proposed login ID is already in use.";
        return result;
    }

    private async Task<SocietyInvitationResult> SendOneAsync(long id)
    {
        var review = await ReviewOneAsync(id);
        if (!review.Eligible) return new() { SocietyId = id, Status = "Needs review", Message = review.Problem };

        var row = await context.Society_Admin_Onboarding.SingleAsync(x => x.Society_Id == id);
        var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
            .Replace('+', 'A').Replace('/', 'b').TrimEnd('=') + "aA1!";
        var now = DateTime.UtcNow;
        try
        {
            await using (var transaction = await context.Database.BeginTransactionAsync())
            {
                await context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(7325901)");
                await context.Entry(row).ReloadAsync();
                if (row.Status == "Sending" || (row.Status == "Sent" && !await context.Users.AnyAsync(x => x.Id == row.User_Id
                    && x.Must_Change_Password && x.Invitation_Expires_At <= DateTime.UtcNow)))
                    return new() { SocietyId = id, Status = "Needs review", Message = "Invitation already sent or in progress." };

                Users? user = row.User_Id is null ? null : await context.Users.FindAsync(row.User_Id.Value);
                if (user is null)
                {
                    var nextId = (await context.Users.MaxAsync(x => (decimal?)x.Id) ?? 11010000000) + 1;
                    user = new Users { Id = nextId, Username = review.LoginId, BrCode = row.BrCode,
                        Role = "Admin", Created_At = now };
                    context.Users.Add(user);
                }
                userHandler.CreatePasswordHash(password, out var hash, out var salt);
                user.Password_Hash = hash;
                user.Password_Salt = salt;
                user.Email = row.Admin_Email;
                user.First_Name = row.Admin_Name;
                user.Is_Active = true;
                user.Is_Account_Closed = false;
                user.Is_Locked = false;
                user.Failed_Login_Attempts = 0;
                user.Must_Change_Password = true;
                user.Invitation_Expires_At = now.AddDays(2);
                user.Security_Stamp = Guid.NewGuid().ToString("N");
                user.Updated_At = now;
                row.User_Id = user.Id;
                row.Status = "Sending";
                row.Last_Attempt_At = now;
                row.Last_Error = null;
                row.Updated_By = OperatorId();
                context.Society_Admin_Invitation_Audit.Add(new()
                {
                    Society_Id = id, User_Id = user.Id, Actor_Id = OperatorId(),
                    Event = "SendAttempt", Occurred_At = now, Recipient = row.Admin_Email
                });
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            using var message = new MailMessage
            {
                From = new MailAddress(configuration["OnboardingMail:From"]!),
                Subject = $"ECS application access for {review.SocietyName.Replace('\r', ' ').Replace('\n', ' ')}",
                Body = $"Dear {row.Admin_Name},\n\nFollowing your society's agreement, access to the ECS application has been prepared for {review.SocietyName}.\n\nApplication: {configuration["OnboardingMail:ApplicationUrl"]}\nUser ID: {review.LoginId}\nTemporary password: {password}\nExpires: {now.AddDays(2):yyyy-MM-dd HH:mm} UTC\n\nPlease sign in and change the password immediately. If you did not expect this message, contact {configuration["OnboardingMail:SupportContact"]}.\n"
            };
            message.To.Add(new MailAddress(row.Admin_Email));
            using var smtp = new SmtpClient(configuration["OnboardingMail:Host"]!, int.Parse(configuration["OnboardingMail:Port"]!))
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(configuration["OnboardingMail:Username"], configuration["OnboardingMail:Password"]),
                Timeout = 30000
            };
            await smtp.SendMailAsync(message, HttpContext.RequestAborted);
            row.Status = "Sent";
            row.Sent_At = DateTime.UtcNow;
            context.Society_Admin_Invitation_Audit.Add(new()
            {
                Society_Id = id, User_Id = row.User_Id, Actor_Id = OperatorId(),
                Event = "ProviderAccepted", Occurred_At = row.Sent_At.Value, Recipient = row.Admin_Email
            });
            await context.SaveChangesAsync();
            return new() { SocietyId = id, Status = "Sent", Message = "Accepted by mail server." };
        }
        catch
        {
            context.ChangeTracker.Clear();
            var failed = await context.Society_Admin_Onboarding.FindAsync(id);
            if (failed is not null && failed.Status == "Sending")
            {
                failed.Status = "Failed";
                failed.Last_Error = "Mail send failed. Check mail configuration and retry.";
                context.Society_Admin_Invitation_Audit.Add(new()
                {
                    Society_Id = id, User_Id = failed.User_Id, Actor_Id = OperatorId(),
                    Event = "SendFailed", Occurred_At = DateTime.UtcNow, Recipient = failed.Admin_Email
                });
                await context.SaveChangesAsync();
            }
            return new() { SocietyId = id, Status = "Failed", Message = "Invitation failed. Check mail settings and retry." };
        }
    }

    private bool MailConfigured() =>
        new[] { "Host", "Port", "Username", "Password", "From", "ApplicationUrl", "SupportContact" }
            .All(key => !string.IsNullOrWhiteSpace(configuration[$"OnboardingMail:{key}"]))
        && int.TryParse(configuration["OnboardingMail:Port"], out _);

    private decimal OperatorId() =>
        decimal.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private bool SameOrigin()
    {
        var origin = Request.Headers.Origin.ToString();
        if (string.IsNullOrWhiteSpace(origin)) return false;
        return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && uri.Scheme == Request.Scheme && uri.Authority == Request.Host.Value;
    }

    private async Task<string?> SocietyNameAsync(long id)
    {
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        await connection.OpenAsync(HttpContext.RequestAborted);
        try
        {
            await using var command = new NpgsqlCommand("SELECT society_name FROM public.societies WHERE society_id=@id", connection);
            command.Parameters.AddWithValue("id", id);
            return (string?)await command.ExecuteScalarAsync(HttpContext.RequestAborted);
        }
        finally { await connection.CloseAsync(); }
    }
}
