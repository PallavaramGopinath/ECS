using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    public interface IDistrictMasterRepository
    {
        List<DropdownItem> GetDistrictItems();
        bool AddDistrict(Refer_District district);
        bool EditDistrict(Refer_District district);
    }
}
