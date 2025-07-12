using Repository.Models;
using Repository.Repositories;
using Service.DTOs;
using Service.Models;
using System.Text.RegularExpressions;

namespace Service.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IBlogPostRepo _blogPostRepo;
        private readonly IUserRepo _userRepo;

        public BlogPostService(IBlogPostRepo blogPostRepo, IUserRepo userRepo)
        {
            _blogPostRepo = blogPostRepo;
            _userRepo = userRepo;
        }

        public async Task<ResultModel> CreateBlogPostAsync(BlogPostCreateDto createDto, int authorId)
        {
            try
            {
                // Verify author exists
                var author = await _userRepo.GetByIdAsync(authorId);
                if (author == null)
                {
                    return ResultModel.NotFound("Author not found");
                }

                var blogPost = new BlogPost
                {
                    Title = createDto.Title,
                    Content = createDto.Content,
                    AuthorId = authorId,
                    PublishedAt = createDto.PublishNow ? DateTime.UtcNow : null
                };

                var createdPost = await _blogPostRepo.CreateAsync(blogPost);
                var blogPostDto = await MapToBlogPostDto(createdPost);

                return ResultModel.Created(blogPostDto, "Blog post created successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to create blog post: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateBlogPostAsync(int postId, BlogPostUpdateDto updateDto, int userId)
        {
            try
            {
                var blogPost = await _blogPostRepo.GetByIdAsync(postId);
                if (blogPost == null)
                {
                    return ResultModel.NotFound("Blog post not found");
                }

                // Check if user is the author or has admin privileges
                if (blogPost.AuthorId != userId)
                {
                    // In a real implementation, you would check user roles here
                    // For now, only allow authors to edit their own posts
                    return ResultModel.Forbidden("You can only edit your own blog posts");
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateDto.Title))
                    blogPost.Title = updateDto.Title;

                if (!string.IsNullOrEmpty(updateDto.Content))
                    blogPost.Content = updateDto.Content;

                if (updateDto.IsPublished.HasValue)
                {
                    if (updateDto.IsPublished.Value && blogPost.PublishedAt == null)
                    {
                        blogPost.PublishedAt = DateTime.UtcNow;
                    }
                    else if (!updateDto.IsPublished.Value)
                    {
                        blogPost.PublishedAt = null;
                    }
                }

                await _blogPostRepo.UpdateAsync(blogPost);
                var updatedPostDto = await MapToBlogPostDto(blogPost);

                return ResultModel.Success(updatedPostDto, "Blog post updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update blog post: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteBlogPostAsync(int postId, int userId)
        {
            try
            {
                var blogPost = await _blogPostRepo.GetByIdAsync(postId);
                if (blogPost == null)
                {
                    return ResultModel.NotFound("Blog post not found");
                }

                // Check if user is the author or has admin privileges
                if (blogPost.AuthorId != userId)
                {
                    return ResultModel.Forbidden("You can only delete your own blog posts");
                }

                await _blogPostRepo.RemoveAsync(blogPost);
                return ResultModel.Success(null, "Blog post deleted successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to delete blog post: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetBlogPostByIdAsync(int postId)
        {
            try
            {
                var blogPost = await _blogPostRepo.GetByIdWithAuthorAsync(postId);
                if (blogPost == null)
                {
                    return ResultModel.NotFound("Blog post not found");
                }

                var blogPostDto = await MapToBlogPostDto(blogPost);
                return ResultModel.Success(blogPostDto);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get blog post: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetAllBlogPostsAsync(BlogPostSearchDto searchDto)
        {
            try
            {
                var allPosts = await _blogPostRepo.GetAllWithAuthorAsync();
                var filteredPosts = ApplyFilters(allPosts, searchDto);
                var paginatedResult = CreatePaginatedResult(filteredPosts, searchDto);

                return ResultModel.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get blog posts: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetPublishedBlogPostsAsync(BlogPostSearchDto searchDto)
        {
            try
            {
                var publishedPosts = await _blogPostRepo.GetPublishedPostsAsync();
                var filteredPosts = ApplyFilters(publishedPosts, searchDto);
                var paginatedResult = CreatePaginatedResult(filteredPosts, searchDto);

                return ResultModel.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get published blog posts: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetBlogPostsByAuthorAsync(int authorId, BlogPostSearchDto searchDto)
        {
            try
            {
                var authorPosts = await _blogPostRepo.GetByAuthorIdAsync(authorId);
                var filteredPosts = ApplyFilters(authorPosts, searchDto);
                var paginatedResult = CreatePaginatedResult(filteredPosts, searchDto);

                return ResultModel.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get blog posts by author: {ex.Message}");
            }
        }

        public async Task<ResultModel> PublishBlogPostAsync(int postId, int userId)
        {
            try
            {
                var blogPost = await _blogPostRepo.GetByIdAsync(postId);
                if (blogPost == null)
                {
                    return ResultModel.NotFound("Blog post not found");
                }

                if (blogPost.AuthorId != userId)
                {
                    return ResultModel.Forbidden("You can only publish your own blog posts");
                }

                if (blogPost.PublishedAt != null)
                {
                    return ResultModel.BadRequest("Blog post is already published");
                }

                blogPost.PublishedAt = DateTime.UtcNow;
                await _blogPostRepo.UpdateAsync(blogPost);

                var blogPostDto = await MapToBlogPostDto(blogPost);
                return ResultModel.Success(blogPostDto, "Blog post published successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to publish blog post: {ex.Message}");
            }
        }

        public async Task<ResultModel> UnpublishBlogPostAsync(int postId, int userId)
        {
            try
            {
                var blogPost = await _blogPostRepo.GetByIdAsync(postId);
                if (blogPost == null)
                {
                    return ResultModel.NotFound("Blog post not found");
                }

                if (blogPost.AuthorId != userId)
                {
                    return ResultModel.Forbidden("You can only unpublish your own blog posts");
                }

                if (blogPost.PublishedAt == null)
                {
                    return ResultModel.BadRequest("Blog post is not published");
                }

                blogPost.PublishedAt = null;
                await _blogPostRepo.UpdateAsync(blogPost);

                var blogPostDto = await MapToBlogPostDto(blogPost);
                return ResultModel.Success(blogPostDto, "Blog post unpublished successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to unpublish blog post: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetRecentBlogPostsAsync(int count)
        {
            try
            {
                var recentPosts = await _blogPostRepo.GetRecentPostsAsync(count);
                var blogPostDtos = recentPosts.Select(MapToBlogPostSummaryDto).ToList();

                return ResultModel.Success(blogPostDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get recent blog posts: {ex.Message}");
            }
        }

        public async Task<ResultModel> SearchBlogPostsAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                var searchResults = await _blogPostRepo.SearchPostsAsync(searchTerm);
                var searchDto = new BlogPostSearchDto
                {
                    SearchTerm = searchTerm,
                    Page = page,
                    PageSize = pageSize
                };

                var paginatedResult = CreatePaginatedResult(searchResults, searchDto);
                return ResultModel.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to search blog posts: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetBlogPostStatsAsync()
        {
            try
            {
                var totalPosts = await _blogPostRepo.GetTotalPostsCountAsync();
                var publishedPosts = await _blogPostRepo.GetPublishedPostsCountAsync();
                var draftPosts = totalPosts - publishedPosts;

                var stats = new
                {
                    TotalPosts = totalPosts,
                    PublishedPosts = publishedPosts,
                    DraftPosts = draftPosts
                };

                return ResultModel.Success(stats);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get blog post stats: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetMyBlogPostsAsync(int authorId, BlogPostSearchDto searchDto)
        {
            try
            {
                searchDto.AuthorId = authorId;
                return await GetBlogPostsByAuthorAsync(authorId, searchDto);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get my blog posts: {ex.Message}");
            }
        }

        private async Task<BlogPostDto> MapToBlogPostDto(BlogPost blogPost)
        {
            var author = blogPost.Author ?? await _userRepo.GetByIdAsync(blogPost.AuthorId ?? 0);

            return new BlogPostDto
            {
                PostId = blogPost.PostId,
                Title = blogPost.Title,
                Content = blogPost.Content,
                AuthorId = blogPost.AuthorId,
                AuthorName = author != null ? $"{author.FirstName} {author.LastName}" : "Unknown Author",
                PublishedAt = blogPost.PublishedAt,
                IsPublished = blogPost.PublishedAt != null,
                Status = blogPost.PublishedAt != null ? "Published" : "Draft"
            };
        }

        private BlogPostSummaryDto MapToBlogPostSummaryDto(BlogPost blogPost)
        {
            var authorName = blogPost.Author != null ? $"{blogPost.Author.FirstName} {blogPost.Author.LastName}" : "Unknown Author";

            return new BlogPostSummaryDto
            {
                PostId = blogPost.PostId,
                Title = blogPost.Title,
                ContentPreview = GetContentPreview(blogPost.Content, 200),
                AuthorId = blogPost.AuthorId,
                AuthorName = authorName,
                PublishedAt = blogPost.PublishedAt,
                IsPublished = blogPost.PublishedAt != null,
                Status = blogPost.PublishedAt != null ? "Published" : "Draft"
            };
        }

        private string GetContentPreview(string content, int maxLength)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            // Remove HTML tags for preview
            var cleanContent = Regex.Replace(content, @"<[^>]*>", string.Empty);

            if (cleanContent.Length <= maxLength)
                return cleanContent;

            return cleanContent.Substring(0, maxLength) + "...";
        }

        private IEnumerable<BlogPost> ApplyFilters(IEnumerable<BlogPost> posts, BlogPostSearchDto searchDto)
        {
            var filteredPosts = posts;

            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                filteredPosts = filteredPosts.Where(p =>
                    p.Title.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Content.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (searchDto.AuthorId.HasValue)
            {
                filteredPosts = filteredPosts.Where(p => p.AuthorId == searchDto.AuthorId.Value);
            }

            if (searchDto.IsPublished.HasValue)
            {
                if (searchDto.IsPublished.Value)
                {
                    filteredPosts = filteredPosts.Where(p => p.PublishedAt != null);
                }
                else
                {
                    filteredPosts = filteredPosts.Where(p => p.PublishedAt == null);
                }
            }

            if (searchDto.StartDate.HasValue)
            {
                filteredPosts = filteredPosts.Where(p => p.PublishedAt >= searchDto.StartDate.Value);
            }

            if (searchDto.EndDate.HasValue)
            {
                filteredPosts = filteredPosts.Where(p => p.PublishedAt <= searchDto.EndDate.Value);
            }

            return filteredPosts;
        }

        public async Task<ResultModel> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var posts = await _blogPostRepo.GetPostsByDateRangeAsync(startDate, endDate);
                var blogPostDtos = posts.Select(MapToBlogPostSummaryDto).ToList();

                return ResultModel.Success(blogPostDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get posts by date range: {ex.Message}");
            }
        }

        private PaginatedBlogPostsDto CreatePaginatedResult(IEnumerable<BlogPost> posts, BlogPostSearchDto searchDto)
        {
            var totalCount = posts.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);

            var paginatedPosts = posts
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(MapToBlogPostSummaryDto)
                .ToList();

            return new PaginatedBlogPostsDto
            {
                Posts = paginatedPosts,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = totalPages,
                HasNextPage = searchDto.Page < totalPages,
                HasPreviousPage = searchDto.Page > 1
            };
        }
    }
}