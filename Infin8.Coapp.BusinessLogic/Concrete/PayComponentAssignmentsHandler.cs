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
    public  class PayComponentAssignmentsHandler : IPayComponentAssignmentsHandler 
    {
        readonly IUnitOfWork _unitOfWork;
        public PayComponentAssignmentsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Pay_Component_Assignments>> AddPayComponentAssignments(Pay_Component_Assignments entity)
        {
            return await _unitOfWork.PayComponentAssignments.AddPayComponentAssignments(entity);
        }

        public async Task<List<Pay_Component_Assignments>> AddPayComponentAssignments(List<Pay_Component_Assignments> entities)
        {
            List<Pay_Component_Assignments> components = new();
            List<Pay_Component_Assignments> componentsForDeletion = new();
            try
            {
                _unitOfWork.BeginTransaction();
                /// delete existing components by empid and component id
                var existingComponents = await _unitOfWork.PayComponentAssignments.GetExistingPayComponentAssignments(entities);

                foreach (var entity in existingComponents)
                {
                    entity.Is_Active = false;
                    //var result = await _unitOfWork.PayComponentAssignments.EditPayComponentassignments(entity);
                }
                var result = await _unitOfWork.PayComponentAssignments.EditPayComponentassignments(existingComponents);
                /// Insert all components
                var finalResult = await _unitOfWork.PayComponentAssignments.AddPayComponentAssignments(entities);
                if (finalResult != null && finalResult.Count >0) { components = finalResult.ToList(); }
                _unitOfWork.Complete();
                _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                _unitOfWork.RollBack();
            }
            return components;
        }

        public async Task<List<Pay_Component_Assignments>> EditPayComponentassignments(Pay_Component_Assignments entity)
        {
            return await _unitOfWork.PayComponentAssignments.EditPayComponentassignments(entity);
        }

        public async Task<List<Pay_Component_Assignments>> GetPayComponentAssignments(string brCode)
        {
            return await _unitOfWork.PayComponentAssignments.GetPayComponentAssignments(brCode);
        }
    }
}
