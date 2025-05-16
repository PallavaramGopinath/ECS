using Infin8.Coapp.BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewAccountNoController : ControllerBase
    {
        readonly INewAccountNoHandler _newAccountNoHandler;
        public NewAccountNoController(INewAccountNoHandler newAccountNoHandler )
        {
            _newAccountNoHandler = newAccountNoHandler;
        }
        [HttpGet]
        [Route("NewMemberNo/{memberType:int}/{brCode}")]
        public async Task<ActionResult<string>> GetNewMemberNo(int memberType,string brCode)
        {
            string newMemberNo = await _newAccountNoHandler.NewMemberNoAsync(memberType, brCode);
            if(string.IsNullOrWhiteSpace(newMemberNo)) return NotFound ();
            else return Ok (newMemberNo);
        }
        [HttpGet]
        [Route("NewTDNo/{schemeId:int}/{brCode}")]
        public async Task<ActionResult<string>> GetNewTDNo(int schemeId, string brCode)
        {
            string newTDNo = await _newAccountNoHandler.NewTDNoAsync(schemeId, brCode);
            if (string.IsNullOrWhiteSpace(newTDNo)) return NotFound();
            else return Ok(newTDNo);
        }
        [HttpGet]
        [Route("NewLoanNo/{schemeId:int}/{brCode}")]
        public async Task<ActionResult<string>> GetNewLoanNo(int schemeId, string brCode)
        {
            string newTDNo = await _newAccountNoHandler.NewTDNoAsync(schemeId, brCode);
            if (string.IsNullOrWhiteSpace(newTDNo)) return NotFound();
            else return Ok(newTDNo);
        }
        [HttpGet]
        [Route("NewSBNo/{schemeId:int}/{brCode}")]
        public async Task<ActionResult<string>> GetNewSBNo(int schemeId, string brCode)
        {
            string newTDNo = await _newAccountNoHandler.NewSBNoAsync(schemeId, brCode);
            if (string.IsNullOrWhiteSpace(newTDNo)) return NotFound();
            else return Ok(newTDNo);
        }
    }
}
