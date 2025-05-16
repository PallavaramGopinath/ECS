using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public class BankMasterHandler : IBankMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public BankMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool AddBankMaster(Bank_Master bankMaster)
        {
           return _unitOfWork.BankMaster.AddBankMaster(bankMaster);
        }

        public bool EditBankMaster(Bank_Master bankMaster)
        {
            return _unitOfWork.BankMaster.EditBankMaster(bankMaster);
        }

        public async Task<List<DropdownItem>> GetBankItemsAsync()
        {
            return await _unitOfWork.BankMaster.GetBankItemsAsync();
        }
    }
}
