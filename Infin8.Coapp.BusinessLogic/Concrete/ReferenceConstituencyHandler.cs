using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic 
{
    public class ReferenceConstituencyHandler : IReferenceConstituencyHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public ReferenceConstituencyHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddConstituency(Refer_Constituency refer_Constituency)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.ReferenceConstituency.AddConstituency(refer_Constituency);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Constituency not saved");
            }
            return result;
        }

        public async  Task<bool> EditConstituency(Refer_Constituency refer_Constituency)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.ReferenceConstituency.EditConstituency(refer_Constituency);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Constituency not modified/deleted");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetConstituencyItems()
        {
            List<DropdownItem> items = new List<DropdownItem>();
            items = await _unitOfWork.ReferenceConstituency.GetConstituencyItems();
            return items;
        }
    }
}
