using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Models;
using Repository.Repositories.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class FeedbackRepo : GenericRepository<Feedback>, IFeedbackRepo
    {
        public FeedbackRepo(GenderHealthcareDBContext context) : base(context) { }

        public async Task<int> GetMaxFeedbackIdAsync()
        {
            return await _dbSet.MaxAsync(f => (int?)f.FeedbackId) ?? 0;
        }

        public async Task<Feedback?> GetFeedbackByUserAndServiceAsync(int userId, int serviceId)
        {
            return await _dbSet.FirstOrDefaultAsync(f => f.UserId == userId && f.RelatedServiceId == serviceId);
        }
    }
}
