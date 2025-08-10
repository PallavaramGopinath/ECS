using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class StagingMasterRepository : Repository<Staging_Master>, IStagingMasterRepository
    {
        /// <summary>
        /// Staging_Status ="I" = initiated by maker
        /// Staging_Status ="M" = submitted by maker
        /// Staging_Status ="R" = rejected by checker
        /// Staging_Status ="V" = verified by checker
        /// </summary>
        public CSISContext CSISContext => (CSISContext)Context;
        public StagingMasterRepository(DbContext context) : base(context)
        {
        }
        public async  Task<decimal> AddStagingMaster(Staging_Master stagingMaster  )
        {
            decimal maxId = 0;
            try
            {
                maxId = await CSISContext.Staging_Master.Where(x => x.BrCode == stagingMaster.BrCode).MaxAsync(x => x.Staging_Id);
                maxId++;
                stagingMaster.Staging_Id = maxId;
                await AddAsync(stagingMaster);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new Staging Master");
            }
            return maxId;
        }

        public bool IsStagingMasterCreated(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate)
        {
            bool result = false;
            var count =  CSISContext.Staging_Master
                .Count(s => s.Member_Id == memId &&
                s.Created_By == createdBy &&
                s.Created_Date == createdDate &&
                s.Staging_Status == stagingStatus);
            if (count == 0) result = false;
            else result = true;
            return result;
        }

        public async Task<bool> DeleteStagingMaster(decimal stagingId)
        {
            bool result = false;
            try
            {
                 await CSISContext.Staging_Master
                .Where(x => x.Staging_Id == stagingId )
                .ExecuteDeleteAsync();
                await CSISContext.SaveChangesAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while deleting Staging Master");
            }
            return result;
        }

        public async Task<decimal> GetStagingMasterId(decimal createdBy, decimal memId, string stagingStatus, DateTime createdDate)
        {
            decimal stagingId = await  CSISContext.Staging_Master
            .Where(s => s.Member_Id == memId &&
                        s.Created_By == createdBy &&
                        s.Created_Date == createdDate &&
                        s.Staging_Status == stagingStatus)
            .Select(s => s.Staging_Id)
            .FirstOrDefaultAsync(); // or .First() if you're sure a record exists
            return stagingId;
        }

        public async Task<bool> MakeStagingMaster(decimal stagingId)
        {
            bool result = false;
            Staging_Master stagingMaster = new Staging_Master();
            try
            {
                var query  = await CSISContext.Staging_Master
                .Where(x => x.Staging_Id == stagingId)
                .FirstOrDefaultAsync();
                if(query != null && query.Staging_Id == stagingId)
                {
                    stagingMaster = query;
                    stagingMaster.Staging_Status = "M"; // Change status to "M" for submitted by maker
                    await CSISContext.SaveChangesAsync();
                    result = true;
                }
                else
                {
                    result = false;
                    throw new InvalidOperationException("Staging Master not found for the given ID.");
                }
            }
            catch (Exception)
            {
                result = false;
                throw new InvalidOperationException("Staging Master not found for the given ID.");
            }
            return result;
        }

        public async Task<bool> CheckerStateStaging(decimal stagingId,decimal vocId, decimal checkedBy, string stagingStatus)
        {
            bool result = false;
            Staging_Master stagingMaster = new Staging_Master();
            try
            {
                var query = await CSISContext.Staging_Master
                .Where(x => x.Staging_Id == stagingId && x.Staging_Status == "M")
                .FirstOrDefaultAsync();
                if (query != null && query.Staging_Id == stagingId)
                {
                    stagingMaster = query;
                    stagingMaster.Voc_Id = vocId;
                    stagingMaster.Checked_By = checkedBy;
                    stagingMaster.Checked_Date = DateTime.Now;
                    stagingMaster.Staging_Status = stagingStatus; // Change status to "M" for submitted by maker
                    CSISContext.Entry(stagingMaster).State = EntityState.Modified;
                    //await EditAsync(stagingMaster);
                    //await CSISContext.SaveChangesAsync();
                    result = true;
                }
                else
                {
                    result = false;
                    throw new InvalidOperationException("Staging Master not found for the given ID.");
                }
            }
            catch (Exception ex)
            {
                string er = ex.Message;
                result = false;
                throw new InvalidOperationException("Staging Master not found for the given ID.");
            }
            return result;

        }
    }
}
