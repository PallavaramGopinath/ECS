using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IBankMasterHandler
    {
        bool AddBankMaster(Bank_Master bankMaster);
        bool EditBankMaster(Bank_Master bankMaster);
        Task<List<DropdownItem>> GetBankItemsAsync();
    }
}
