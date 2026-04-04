using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
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

        #region Reference Data
        [HttpGet]
        [Route("GetReferenceData/{brCode}")]
        public async Task<ActionResult<List<Refer_Data>>> GetReferenceData(string brCode)
        {
            List<Refer_Data> items = new();
            var response = await _referenceHandler.GetReferences(brCode);
            if (response != null && response.Count > 0) items = response.ToList();
            return Ok(items);
        }

        [HttpPost]
        [Route("AddReferenceData")]
        public async Task<ActionResult<List<Refer_Data>>> AddReferenceData([FromBody] Refer_Data referData)
        {
            List<Refer_Data> items = new();
            var response = await _referenceHandler.AddReference(referData);
             if (response != null && response.Count > 0) items = response.ToList();
            return Ok(items);
        }

        [HttpPost]
        [Route("EditReferenceData")]
        public async Task<ActionResult<List<Refer_Data>>> EditReferenceData([FromBody] Refer_Data referData)
        {
            List<Refer_Data> items = new();
            var response = await _referenceHandler.EditReference (referData);
            if (response != null && response.Count > 0) items = response.ToList();
            return Ok(items);
        }
        #endregion

        #region Reference Area
        [HttpGet]
        [Route("GetReferenceArea/{brCode}")]
        public async Task<ActionResult<List<Refer_Area>>> GetReferenceArea(string brCode)
        {
            List<Refer_Area> items = new();
            var response = await _areaMasterHandler.GetAreas(brCode);
            if (response != null && response.Count > 0) items = response.ToList();
            return Ok(items);
        }

        [HttpGet]
        [Route("GetReferenceAreaWithTalukDistrict/{brCode}")]
        public async Task<ActionResult<List<DtoReferArea>>> GetReferenceAreaWithTalukDistrict(string brCode)
        {
            List<DtoReferArea> items = new();
            var response = await _areaMasterHandler.GetAreasWithTalukDistrictNames(brCode);
            if (response != null && response.Count > 0) items = response.ToList();
            return Ok(items);
        }

        [HttpPost]
        [Route("AddReferenceArea")]
        public async Task<ActionResult<List<Refer_Area>>> AddReferenceArea([FromBody] Refer_Area referArea)
        {
            List<Refer_Area> items = new();
            var response = await _areaMasterHandler.AddArea(referArea);
             if (response != null && response.Count > 0) items = response.ToList();
            return Ok(items);
        }

        [HttpPost]
        [Route("EditReferenceArea")]
        public async Task<ActionResult<List<Refer_Data>>> EditReferenceArea([FromBody] Refer_Area referArea)
        {
            List<Refer_Area> items = new();
            var response = await _areaMasterHandler.EditArea (referArea);
            if (response != null && response.Count > 0) items = response.ToList();
            return Ok(items);
        }

        [HttpGet]
        [Route("GetTaluks/{brCode}")]
        public async Task<ActionResult<List<Refer_Taluk>>> GetTaluks(string brCode)
        {
            List<Refer_Taluk> items = new();
            var response = await _areaMasterHandler.GetTaluks(brCode);
            if (response != null && response.Count > 0) items = response.ToList();
            return Ok(items);
        }
        [HttpGet]
        [Route("GetDistricts/{brCode}")]
        public async Task<ActionResult<List<Refer_District>>> GetDistricts(string brCode)
        {
            List<Refer_District> items = new();
            var response = await _areaMasterHandler.GetDistricts(brCode);
            if (response != null && response.Count > 0) items = response.ToList();
            return Ok(items);
        }
        #endregion 
    }
}
