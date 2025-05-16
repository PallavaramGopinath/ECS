using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    public interface IReferenceRepository
    {
        Task<List<DropdownItem>> GetReferenceItems(int refType, string brCode, bool factoryRec);
        Task<bool> AddReference(Refer_Data referData,string brCode);
        Task<bool> EditReference(Refer_Data referData);

    }
}
