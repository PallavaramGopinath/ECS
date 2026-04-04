using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    public  interface IAreaMasterRepository
    {
        Task<List<DropdownItem>> GetAreaItems(string brCode);
        Task<List<Refer_Area>> AddArea(Refer_Area area);
        Task<List<Refer_Area>> EditArea(Refer_Area area);
        Task<List<Refer_Area>> GetAreas(string brCode);
    }
}
