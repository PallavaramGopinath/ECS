using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public class ReferenceHandler : IReferenceHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public ReferenceHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddReference(Refer_Data referData, string brCode)
        {
            bool result = false;
            try
            {
                result = await  _unitOfWork.References.AddReference(referData,brCode);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan eligible not saved");
            }
            return result;
        }

        public async Task<bool> EditReference(Refer_Data referData)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.References.EditReference(referData);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan eligible not deleted");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetReferenceItems(int refType, string brCode, bool factoryRec)
        {
            List<DropdownItem> items = new List<DropdownItem>();

            items = await  _unitOfWork.References.GetReferenceItems(refType, brCode, factoryRec);
            return items;
        }
        
    }
}
