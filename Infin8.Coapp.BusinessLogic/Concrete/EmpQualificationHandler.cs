using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class EmpQualificationHandler : IEmpQualificationHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public EmpQualificationHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddEmployeeQualificationAsyn(Emp_Qualification empQualification)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeeQualification.AddEmployeeQualificationAsyn(empQualification);                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee qualifications not saved");
            }
            return result;
        }

        public async Task<bool> AddEmployeeQualificationListAsync(List<Emp_Qualification> empQualifications)
        {
            return await _unitOfWork.EmployeeQualification.AddEmployeeQualificationListAsync(empQualifications);
        }

        public async Task<bool> EditEmployeeQualificationAsyn(Emp_Qualification empQualification)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeeQualification.EditEmployeeQualificationAsyn(empQualification);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee qualifications not deleted");
            }
            return result;
        }
    }
}
