using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferenceController : ControllerBase
    {
        readonly IReferenceHandler _referenceHandler;
        readonly IAreaMasterHandler _areaMasterHandler;
        readonly IBankMasterHandler _bankMasterHandler;
        readonly IReferenceConstituencyHandler _referenceConstituencyHandler;
        //readonly IDistrictMasterHandler _districtHandler;
        /// <summary>
        /// , IDistrictMasterHandler districtHandler
        /// </summary>
        /// <param name="referenceHandler"></param>

        public ReferenceController(IReferenceHandler referenceHandler, IAreaMasterHandler areaMasterHandler, 
            IBankMasterHandler bankMasterHandler, IReferenceConstituencyHandler referenceConstituencyHandler)
        {
            _referenceHandler = referenceHandler;
            _areaMasterHandler = areaMasterHandler;
            _bankMasterHandler = bankMasterHandler;
            _referenceConstituencyHandler = referenceConstituencyHandler;
            //_districtHandler = districtHandler;
        }
        [HttpGet]
        [Route ("{refType:int}/{brCode}/{factoryRec:bool}")]
        public async Task<ActionResult<List<DropdownItem>>> GetReferenceItems(int refType, string brCode, bool factoryRec) 
        {
            List<DropdownItem> items = new List<DropdownItem>();
            items = await _referenceHandler.GetReferenceItems(refType, brCode, factoryRec);
            if (items.Count > 0) return  Ok( await Task.FromResult(items));
            else return NotFound();
        }

        //[HttpGet]
        //[Route ("District")]
        //public ActionResult<List<DropdownItem>> GetDistrictItems()
        //{
        //    List<DropdownItem> items = new List<DropdownItem>();

        //    items = _districtHandler.GetDistrictItems();
        //    if (items.Count > 0) return Ok(items);
        //    else return NotFound();
        //}
        [HttpGet]
        [Route("Area/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetAreaItems(string brCode)
        {
            List<DropdownItem> items = new List<DropdownItem>();
            items =  await _areaMasterHandler.GetAreaItems(brCode);
            if (items.Count > 0) return Ok(items);
            else return NotFound();
        }

        [HttpGet]
        [Route ("BankItems")]
        public async Task<ActionResult<List<DropdownItem>>> GetBankItems()
        {
            List<DropdownItem> items = new List<DropdownItem>();
            items = await  _bankMasterHandler.GetBankItemsAsync();
            if (items.Count > 0) return Ok(items);
            else return NotFound();
        }

        [HttpGet]
        [Route("Constituencies")]
        public async Task<ActionResult<List<DropdownItem>>> GetConstituencis()
        {
            List<DropdownItem> items = new List<DropdownItem>();
            items = await _referenceConstituencyHandler.GetConstituencyItems();
            if(items.Count > 0) return Ok(items.ToList());
            else return NotFound();
        }

    }
}
