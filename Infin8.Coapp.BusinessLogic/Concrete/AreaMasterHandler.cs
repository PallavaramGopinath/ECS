using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public  class AreaMasterHandler : IAreaMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public AreaMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Refer_Area>> AddArea(Refer_Area area)
        {
            return await _unitOfWork.Area.AddArea(area);
        }

        public async Task<List<Refer_Area>> EditArea(Refer_Area area)
        {
            return await _unitOfWork.Area.EditArea(area);
        }

        public async Task<List<DropdownItem>> GetAreaItems(string brCode)
        {
            return await _unitOfWork.Area.GetAreaItems(brCode);
        }

        public async Task<List<Refer_Area>> GetAreas(string brCode)
        {
            return await _unitOfWork.Area.GetAreas(brCode);
        }

        public async Task<List<DtoReferArea>> GetAreasWithTalukDistrictNames(string brCode)
        {
            return await _unitOfWork.Area.GetAreasWithTalukDistrictNames(brCode);
        }

        public async Task<List<Refer_District>> GetDistricts(string brCode)
        {
            return await _unitOfWork.Area.GetDistricts(brCode);
        }

        public async Task<List<Refer_Taluk>> GetTaluks(string brCode)
        {
            return await _unitOfWork.Area.GetTaluks(brCode);
        }
    }
}
