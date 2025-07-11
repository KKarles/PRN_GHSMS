using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface IBlogPostService
    {
        Task<ResultModel> CreateBlogPostAsync(BlogPostCreateDto createDto, int authorId);
        Task<ResultModel> UpdateBlogPostAsync(int postId, BlogPostUpdateDto updateDto, int userId);
        Task<ResultModel> DeleteBlogPostAsync(int postId, int userId);
        Task<ResultModel> GetBlogPostByIdAsync(int postId);
        Task<ResultModel> GetAllBlogPostsAsync(BlogPostSearchDto searchDto);
        Task<ResultModel> GetPublishedBlogPostsAsync(BlogPostSearchDto searchDto);
        Task<ResultModel> GetBlogPostsByAuthorAsync(int authorId, BlogPostSearchDto searchDto);
        Task<ResultModel> PublishBlogPostAsync(int postId, int userId);
        Task<ResultModel> UnpublishBlogPostAsync(int postId, int userId);
        Task<ResultModel> GetRecentBlogPostsAsync(int count);
        Task<ResultModel> SearchBlogPostsAsync(string searchTerm, int page = 1, int pageSize = 10);
        Task<ResultModel> GetBlogPostStatsAsync();
        Task<ResultModel> GetMyBlogPostsAsync(int authorId, BlogPostSearchDto searchDto);
        Task<ResultModel> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}