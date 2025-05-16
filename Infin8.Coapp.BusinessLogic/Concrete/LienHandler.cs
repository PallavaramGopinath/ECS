using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class LienHandler : ILienHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LienHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLienAsync(Lien lien)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.Lien.AddLienAsync(lien);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Lien not saved");
            }
            return result;
        }

        public async Task<bool> EditLienAsync(Lien lien)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.Lien.EditLienAsync(lien);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Lien scheme group not deleted");
            }
            return result;
        }
    }
}
