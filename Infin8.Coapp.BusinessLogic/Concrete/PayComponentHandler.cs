using Infin8.Coapp.BusinessLogic.Interface;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic.Concrete
{
    public  class PayComponentHandler : IPayComponentHandler 
    {
        readonly IUnitOfWork _unitOfWork;
        public PayComponentHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Pay_Components>> AddPayComponent(Pay_Components component)
        {
            return await _unitOfWork.PayComponent.AddPayComponent(component);
        }

        public async Task<List<Pay_Components>> EditPayComponent(Pay_Components component)
        {
            return await _unitOfWork.PayComponent.EditPayComponent(component);
        }

        public async Task<List<Pay_Components>> GetPayComponents(string brCode)
        {
            return await _unitOfWork.PayComponent.GetPayComponents(brCode);
        }
    }
}
