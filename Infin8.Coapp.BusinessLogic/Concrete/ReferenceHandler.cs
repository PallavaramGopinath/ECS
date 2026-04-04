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

        public async Task<List<Refer_Data>> AddReference(Refer_Data referData)
        {
            List<Refer_Data> references = new();
            try
            {
                var result = await  _unitOfWork.References.AddReference(referData);
                await _unitOfWork.CompleteAsync();
                if(result != null && result.Count > 0)
                {
                    references = result.ToList ();
                }   
            }
            catch (Exception ex)
            {
                references = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! Reference not saved");
            }
            return references;
        }

        public async Task<List<Refer_Data>> EditReference(Refer_Data referData)
        {
            List<Refer_Data> references = new();
            try
            {
                var result = await _unitOfWork.References.EditReference(referData);
                await _unitOfWork.CompleteAsync();
                if (result != null && result.Count > 0)
                {
                    references = result.ToList();
                }
            }
            catch (Exception ex)
            {
                references = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! Reference not deleted");
            }
            return references;
        }

        public async Task<List<DropdownItem>> GetReferenceItems(int refType, string brCode, bool factoryRec)
        {
            List<DropdownItem> items = new List<DropdownItem>();

            items = await  _unitOfWork.References.GetReferenceItems(refType, brCode, factoryRec);
            return items;
        }

        public async Task<List<Refer_Data>> GetReferences(string brCode)
        {
            return await _unitOfWork.References.GetReferences(brCode);
        }
    }
}
