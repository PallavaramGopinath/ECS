using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository.Interface
{
    public  interface IPayComponentAssignmentsRepository
    {
        Task<List<Pay_Component_Assignments>> AddPayComponentAssignments(Pay_Component_Assignments entity);
        Task<List<Pay_Component_Assignments>> AddPayComponentAssignments(List<Pay_Component_Assignments> entities);
        Task<List<Pay_Component_Assignments>> EditPayComponentassignments(Pay_Component_Assignments entity);
        Task<List<Pay_Component_Assignments>> EditPayComponentassignments(List<Pay_Component_Assignments> entities);
        Task<List<Pay_Component_Assignments>> GetPayComponentAssignments(string brCode);
        Task<List<Pay_Component_Assignments>> GetExistingPayComponentAssignments(List<Pay_Component_Assignments> entities);
    }
}
