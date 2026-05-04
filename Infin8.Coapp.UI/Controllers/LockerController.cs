using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.UI.Client.Pages.Lockers;
using Infin8.Coapp.Models;
namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LockerController : ControllerBase
    {
        readonly ILockerAllotmentsHandler _lockerAllotmentsHandler;
        readonly ILockersHandler _lockersHandler;
        readonly ILockerClosuresHandler _lockerClosuresHandler;
        public LockerController(ILockerAllotmentsHandler lockerAllotmentsHandler, ILockersHandler lockersHandler, ILockerClosuresHandler lockerClosuresHandler)
        {
            _lockerAllotmentsHandler = lockerAllotmentsHandler;
            _lockersHandler = lockersHandler;
            _lockerClosuresHandler = lockerClosuresHandler;
        }

        [HttpGet]
        [Route("GetLockerAllotments/{brCode}")]
        public async Task<ActionResult<List<LockerAllotmentVM>>> GetLockerAllotments(string brCode)
        {
            List<LockerAllotmentVM> lockerAllotments = new List<LockerAllotmentVM>();
            var result = await _lockerAllotmentsHandler.GetLockerAllotmentList(brCode);
            if (result != null || result!.Count > 0)
            {
                lockerAllotments = result.ToList();
            }
            return Ok(lockerAllotments);
        }

        [HttpGet]
        [Route("GetLockerAllotmentByAllotmentId/{id}/{brCode}")]
        public async Task<ActionResult<LockerAllotmentVM>> GetLockerAllotmentByAllotmentId(decimal id, string brCode)
        {
            LockerAllotmentVM lockerAllotment = new LockerAllotmentVM();
            var result = await _lockerAllotmentsHandler.GetLockerAllotmentByAllotmentId(id, brCode);
            if (result != null && result.Id > 0)
            {
                lockerAllotment = result;
            }
            return Ok(lockerAllotment);
        }

        [HttpGet]
        [Route("GetAvailableLockers/{brCode}")]
        public async Task<ActionResult<List<LockerLookupDto>>> GetAvailableLockers(string brCode)
        {
            List<LockerLookupDto> lockers = new List<LockerLookupDto>();
            var result = await _lockersHandler.GetAvailableLockerList(brCode);
            if (result != null && result.Count > 0)
            {
                lockers = result
                    .Select(l => new LockerLookupDto
                    {
                        Id = l.Id,
                        LockerNumber = l.LockerNumber ?? string.Empty,
                        Size = l.Size ?? string.Empty,
                        RentAmount = l.RentAmount,
                        DepositAmount = l.DepositAmount,
                        Status = l.Status ?? string.Empty
                    })
                    .ToList();
            }
            return Ok(lockers);
        }

        //[HttpPost]
        //[Route("AddLockerAllotment")]
        //public async Task<ActionResult<(bool result, decimal allotmentId)>> AddLockerAllotment([FromBody] LockerAllotmentVM lockerAllotment)
        //{
        //    bool result = false;
        //    decimal allotmentId = 0;
        //    var respondResult = await _lockerAllotmentsHandler.AddLockerAllotment(lockerAllotment);
        //    result = respondResult.result;
        //    allotmentId = respondResult.allotmentId;
        //    return Ok(result,allotmentId);
        //}

        [HttpPost]
        [Route("CloseLockerAllotment")]
        public async Task<ActionResult<List<LockerAllotmentVM>>> CloseLockerAllotment([FromBody] Locker_Closures lockerClosure)
        {
            List<LockerAllotmentVM> lockerAllotments = new List<LockerAllotmentVM>();
            var result = await _lockerClosuresHandler.CloseLockerAllotment(lockerClosure);
            if (result != null && result.Count > 0)
            {
                lockerAllotments = result.ToList();
            }
            return Ok(lockerAllotments);
        }

    }
}
