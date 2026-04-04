using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IReferenceHandler
    {
        Task<List<Refer_Data>> GetReferences(string brCode);
        Task<List<Refer_Data>> AddReference(Refer_Data referData);
        Task<List<Refer_Data>> EditReference(Refer_Data referData);
        Task<List<DropdownItem>> GetReferenceItems(int refType, string brCode, bool factoryRec);

    }
}
