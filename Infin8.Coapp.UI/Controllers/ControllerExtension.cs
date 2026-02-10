using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Infin8.Coapp.UI.Controllers
{
    public static class ControllerExtensions
    {
        public static UserInfoDto GetUserInfoDto(this ControllerBase controller)
        {
            var userInfoDto = new UserInfoDto();
            //if (controller.User.Identity != null && controller.User.Identity.IsAuthenticated)
            if (controller.User != null)
            {
                var claimsIdentity = controller.User.Identity as System.Security.Claims.ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    userInfoDto.UserId = decimal.Parse(claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    userInfoDto.Username = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value!;
                    userInfoDto.BrCode = claimsIdentity.FindFirst("BrCode")?.Value;
                    userInfoDto.YrId = decimal.Parse(claimsIdentity.FindFirst("YrId")?.Value ?? "0");
                    userInfoDto.YrBeginningDate = DateTime.Parse(claimsIdentity.FindFirst("YrBeginningDate")?.Value ?? DateTime.MinValue.ToString());
                    userInfoDto.YrEndDate = DateTime.Parse(claimsIdentity.FindFirst("YrEndDate")?.Value ?? DateTime.MinValue.ToString());
                    userInfoDto.CurrentDate = DateTime.Parse(claimsIdentity.FindFirst("CurrentDate")?.Value ?? DateTime.MinValue.ToString());
                    userInfoDto.IsAuthenticated = controller.User.Identity.IsAuthenticated;
                    foreach (var claim in claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.Role))
                    {
                        userInfoDto.Roles.Add(claim.Value);
                    }
                }
            }
            return userInfoDto;
        }
    }
}
