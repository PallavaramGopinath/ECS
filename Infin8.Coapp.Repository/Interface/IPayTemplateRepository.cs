using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IPayTemplateRepository
    {
        Task<bool> AddPayTemplateAsync(Pay_Template payTemplate);
        Task<bool> EditPayTemplateAsync(Pay_Template payTemplate);
        Task<Pay_Template> GetPayTemplateAsync();

        Task<Pay_Template> GetPayTemplateAsync(string brCode);


    }
}
