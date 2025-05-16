using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IReferenceHandler
    {
        Task<bool> AddReference(Refer_Data referData, string brCode);
        Task<bool> EditReference(Refer_Data referData);
        Task<List<DropdownItem>> GetReferenceItems(int refType, string brCode, bool factoryRec);

    }
}
