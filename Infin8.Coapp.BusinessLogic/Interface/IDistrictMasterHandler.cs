using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IDistrictMasterHandler
    {
        List<DropdownItem> GetDistrictItems();
        bool AddDistrict(Refer_District district);
        bool EditDistrict(Refer_District district);
    }
}
