//using BusinessLogic;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public class VerifyHandler : IVerifyHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public VerifyHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public bool VerifyDuplicateAccountNo(string accountNo, string tableName, string searchField, string deleteField)
        {
            return _unitOfWork.Verify.VerifyDuplicateAccountNo(accountNo, tableName, searchField, deleteField);
        }

        public bool VerifyNomenClature(string accountNo, string accountType, int loanSchemeId, string brCode, out string errorMessage)
        {
            return _unitOfWork.Verify.VerifyNomenClature(accountNo, accountType, loanSchemeId, brCode, out errorMessage);
        }
    }
}
