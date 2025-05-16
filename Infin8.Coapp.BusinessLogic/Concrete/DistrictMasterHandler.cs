using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public class DistrictMasterHandler : IDistrictMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public DistrictMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public bool AddDistrict(Refer_District district)
        {
            throw new NotImplementedException();
        }

        public bool EditDistrict(Refer_District district)
        {
            throw new NotImplementedException();
        }

        public List<DropdownItem> GetDistrictItems()
        {
            List<DropdownItem> items = new List<DropdownItem>();
            items = _unitOfWork.District.GetDistrictItems();
            return items;
        }
    }
}
