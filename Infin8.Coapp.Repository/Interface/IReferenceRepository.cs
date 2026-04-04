using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    public interface IReferenceRepository
    {
        Task<List<DropdownItem>> GetReferenceItems(int refType, string brCode, bool factoryRec);

        Task<List<Refer_Data>> GetReferences(string brCode);
        Task<List<Refer_Data>> AddReference(Refer_Data referData);
        Task<List<Refer_Data>> EditReference(Refer_Data referData);

    }
}
