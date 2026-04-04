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
    }
}
