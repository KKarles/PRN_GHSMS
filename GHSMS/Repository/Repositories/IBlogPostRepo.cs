using Repository.Base;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public interface IBlogPostRepo : IGenericRepository<BlogPost>
    {
        Task<BlogPost?> GetByIdWithAuthorAsync(int postId);
        Task<IEnumerable<BlogPost>> GetAllWithAuthorAsync();
        Task<IEnumerable<BlogPost>> GetAllWithAuthorAsync(params Expression<Func<BlogPost, object>>[] includes);
        Task<IEnumerable<BlogPost>> GetByAuthorIdAsync(int authorId);
        Task<IEnumerable<BlogPost>> GetPublishedPostsAsync();
        Task<IEnumerable<BlogPost>> GetPublishedPostsByAuthorAsync(int authorId);
        Task<IEnumerable<BlogPost>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<BlogPost>> SearchPostsAsync(string searchTerm);
        Task<IEnumerable<BlogPost>> GetRecentPostsAsync(int count);
        Task<bool> IsAuthorOwnerAsync(int postId, int userId);
        Task<IEnumerable<BlogPost>> GetPaginatedPostsAsync(int page, int pageSize);
        Task<int> GetTotalPostsCountAsync();
        Task<int> GetPublishedPostsCountAsync();
    }
}
