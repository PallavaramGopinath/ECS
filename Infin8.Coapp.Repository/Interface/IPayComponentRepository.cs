using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository.Interface
{
    public  interface IPayComponentRepository
    {
        Task<List<Pay_Components>> AddPayComponent(Pay_Components component);
        Task<List<Pay_Components>> EditPayComponent(Pay_Components component);
        Task<List<Pay_Components>> GetPayComponents(string brCode);
    }
}
