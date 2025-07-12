using Repository.Base;
using Repository.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repository.Repositories
{
    public class BlogPostRepo : GenericRepository<BlogPost>, IBlogPostRepo
    {
        public BlogPostRepo(GenderHealthcareDBContext context) : base(context)
        {
        }

        public async Task<BlogPost?> GetByIdWithAuthorAsync(int postId)
        {
            return await _dbSet
                .Include(bp => bp.Author)
                .FirstOrDefaultAsync(bp => bp.PostId == postId);
        }

        public async Task<IEnumerable<BlogPost>> GetAllWithAuthorAsync()
        {
            return await _dbSet
                .Include(bp => bp.Author)
                .OrderByDescending(bp => bp.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetAllWithAuthorAsync(params Expression<Func<BlogPost, object>>[] includes)
        {
            IQueryable<BlogPost> query = _dbSet.Include(bp => bp.Author);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.OrderByDescending(bp => bp.PublishedAt).ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetByAuthorIdAsync(int authorId)
        {
            return await _dbSet
                .Include(bp => bp.Author)
                .Where(bp => bp.AuthorId == authorId)
                .OrderByDescending(bp => bp.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedPostsAsync()
        {
            return await _dbSet
                .Include(bp => bp.Author)
                .Where(bp => bp.PublishedAt != null)
                .OrderByDescending(bp => bp.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedPostsByAuthorAsync(int authorId)
        {
            return await _dbSet
                .Include(bp => bp.Author)
                .Where(bp => bp.AuthorId == authorId && bp.PublishedAt != null)
                .OrderByDescending(bp => bp.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(bp => bp.Author)
                .Where(bp => bp.PublishedAt != null &&
                           bp.PublishedAt >= startDate &&
                           bp.PublishedAt <= endDate)
                .OrderByDescending(bp => bp.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> SearchPostsAsync(string searchTerm)
        {
            return await _dbSet
                .Include(bp => bp.Author)
                .Where(bp => bp.PublishedAt != null &&
                           (bp.Title.Contains(searchTerm) ||
                            bp.Content.Contains(searchTerm) ||
                            bp.Author.FirstName.Contains(searchTerm) ||
                            bp.Author.LastName.Contains(searchTerm)))
                .OrderByDescending(bp => bp.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetRecentPostsAsync(int count)
        {
            return await _dbSet
                .Include(bp => bp.Author)
                .Where(bp => bp.PublishedAt != null)
                .OrderByDescending(bp => bp.PublishedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> IsAuthorOwnerAsync(int postId, int userId)
        {
            return await _dbSet
                .AnyAsync(bp => bp.PostId == postId && bp.AuthorId == userId);
        }

        public async Task<IEnumerable<BlogPost>> GetPaginatedPostsAsync(int page, int pageSize)
        {
            return await _dbSet
                .Include(bp => bp.Author)
                .Where(bp => bp.PublishedAt != null)
                .OrderByDescending(bp => bp.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalPostsCountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<int> GetPublishedPostsCountAsync()
        {
            return await _dbSet.CountAsync(bp => bp.PublishedAt != null);
        }
    }
}