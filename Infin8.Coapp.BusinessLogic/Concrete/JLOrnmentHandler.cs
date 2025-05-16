using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class JLOrnmentHandler : IJLOrnmentHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public JLOrnmentHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddJLOrnmentListAsync(List<JL_Ornments> jLOrnmentList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.JLOrnment.AddJLOrnmentListAsync(jLOrnmentList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan ornment list not saved");
            }
            return result;
        }

        public async Task<bool> EditJLOrnmentListAsync(List<JL_Ornments> jLOrnmentList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.JLOrnment.EditJLOrnmentListAsync(jLOrnmentList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan ornment list not deleted");
            }
            return result;
        }
    }
}
