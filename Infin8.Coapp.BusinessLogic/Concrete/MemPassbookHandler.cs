using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class MemPassbookHandler : IMemPassbookHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public  MemPassbookHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMemPassbookAsync(Mem_PassBook memPassbook)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemPassbook.AddMemPassbookAsync(memPassbook);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pass book master not saved");
            }
            return result;
        }

        public async Task<bool> DeleteMemPassbookAsync(Mem_PassBook memPassbook)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemPassbook.DeleteMemPassbookAsync(memPassbook);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pass book master not deleted");
            }
            return result;
        }
    }
}
