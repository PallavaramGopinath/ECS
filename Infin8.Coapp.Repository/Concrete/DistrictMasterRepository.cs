using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    public class DistrictMasterRepository : Repository<Refer_District>, IDistrictMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public DistrictMasterRepository(CSISContext context) : base(context)
        {
        }
        public List<DropdownItem> GetDistrictItems()
        {
            List<DropdownItem> items = new List<DropdownItem>();
            try
            {
                var data = from r in CSISContext.Refer_District
                           select new DropdownItem
                           {
                               Value = r.District_Id.ToString(),
                               Text = r.District_Name
                           };
                if (data != null)
                {
                    items = data.ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return items;
        }

        public bool AddDistrict(Refer_District district)
        {
            Add(district);
            return true;
        }

        public bool EditDistrict(Refer_District district)
        {
            Edit(district);
            return true;
        }
    }
}
