using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class GeneralHandler : IGeneralHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public GeneralHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GetSocietyName(string brCode)
        {
            return await  _unitOfWork.General.GetSocietyName(brCode);
        }
    }
}
