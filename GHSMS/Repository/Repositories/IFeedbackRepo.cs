using Repository.Base;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    namespace Repository.Repositories
    {
        public interface IFeedbackRepo : IGenericRepository<Feedback>
        {
            Task<int> GetMaxFeedbackIdAsync();

            Task<Feedback?> GetFeedbackByUserAndServiceAsync(int userId, int serviceId);
        }

    }
}
