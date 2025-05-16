using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    public  interface IAreaMasterRepository
    {
        Task<List<DropdownItem>> GetAreaItems(string brCode);
        bool AddArea(Refer_Area area);
        bool EditArea(Refer_Area area);
    }
}
