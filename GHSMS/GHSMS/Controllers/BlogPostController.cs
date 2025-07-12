using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Services;
using System.Security.Claims;

namespace GHSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogPostController : ControllerBase
    {
        private readonly IBlogPostService _blogPostService;
        private readonly ILogger<BlogPostController> _logger;

        public BlogPostController(IBlogPostService blogPostService, ILogger<BlogPostController> logger)
        {
            _blogPostService = blogPostService;
            _logger = logger;
        }

        /// <summary>
        /// Get all published blog posts (Public endpoint)
        /// </summary>
        [HttpGet("published")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedBlogPosts([FromQuery] BlogPostSearchDto searchDto)
        {
            try
            {
                var result = await _blogPostService.GetPublishedBlogPostsAsync(searchDto);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published blog posts");
                return StatusCode(500, new { message = "Failed to get published blog posts" });
            }
        }

        /// <summary>
        /// Get a specific blog post by ID (Public endpoint)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBlogPostById(int id)
        {
            try
            {
                var result = await _blogPostService.GetBlogPostByIdAsync(id);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog post by ID: {PostId}", id);
                return StatusCode(500, new { message = "Failed to get blog post" });
            }
        }

        /// <summary>
        /// Search published blog posts (Public endpoint)
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchBlogPosts([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _blogPostService.SearchBlogPostsAsync(searchTerm, page, pageSize);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching blog posts");
                return StatusCode(500, new { message = "Failed to search blog posts" });
            }
        }

        /// <summary>
        /// Get recent blog posts (Public endpoint)
        /// </summary>
        [HttpGet("recent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecentBlogPosts([FromQuery] int count = 5)
        {
            try
            {
                var result = await _blogPostService.GetRecentBlogPostsAsync(count);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent blog posts");
                return StatusCode(500, new { message = "Failed to get recent blog posts" });
            }
        }

        /// <summary>
        /// Create a new blog post (Staff, Manager, Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> CreateBlogPost([FromBody] BlogPostCreateDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _blogPostService.CreateBlogPostAsync(createDto, userId.Value);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog post");
                return StatusCode(500, new { message = "Failed to create blog post" });
            }
        }

        /// <summary>
        /// Update a blog post (Author or Admin/Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> UpdateBlogPost(int id, [FromBody] BlogPostUpdateDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _blogPostService.UpdateBlogPostAsync(id, updateDto, userId.Value);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog post: {PostId}", id);
                return StatusCode(500, new { message = "Failed to update blog post" });
            }
        }

        /// <summary>
        /// Delete a blog post (Author or Admin/Manager only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> DeleteBlogPost(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _blogPostService.DeleteBlogPostAsync(id, userId.Value);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog post: {PostId}", id);
                return StatusCode(500, new { message = "Failed to delete blog post" });
            }
        }

        /// <summary>
        /// Publish a blog post (Author or Admin/Manager only)
        /// </summary>
        [HttpPatch("{id}/publish")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> PublishBlogPost(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _blogPostService.PublishBlogPostAsync(id, userId.Value);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing blog post: {PostId}", id);
                return StatusCode(500, new { message = "Failed to publish blog post" });
            }
        }

        /// <summary>
        /// Unpublish a blog post (Author or Admin/Manager only)
        /// </summary>
        [HttpPatch("{id}/unpublish")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> UnpublishBlogPost(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _blogPostService.UnpublishBlogPostAsync(id, userId.Value);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing blog post: {PostId}", id);
                return StatusCode(500, new { message = "Failed to unpublish blog post" });
            }
        }

        /// <summary>
        /// Get all blog posts (Admin/Manager only)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetAllBlogPosts([FromQuery] BlogPostSearchDto searchDto)
        {
            try
            {
                var result = await _blogPostService.GetAllBlogPostsAsync(searchDto);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all blog posts");
                return StatusCode(500, new { message = "Failed to get all blog posts" });
            }
        }

        /// <summary>
        /// Get my blog posts (Staff, Manager, Admin only)
        /// </summary>
        [HttpGet("my-posts")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> GetMyBlogPosts([FromQuery] BlogPostSearchDto searchDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _blogPostService.GetMyBlogPostsAsync(userId.Value, searchDto);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my blog posts");
                return StatusCode(500, new { message = "Failed to get my blog posts" });
            }
        }

        /// <summary>
        /// Get blog posts by author (Public endpoint)
        /// </summary>
        [HttpGet("author/{authorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBlogPostsByAuthor(int authorId, [FromQuery] BlogPostSearchDto searchDto)
        {
            try
            {
                var result = await _blogPostService.GetBlogPostsByAuthorAsync(authorId, searchDto);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog posts by author: {AuthorId}", authorId);
                return StatusCode(500, new { message = "Failed to get blog posts by author" });
            }
        }

        /// <summary>
        /// Get blog post statistics (Admin/Manager only)
        /// </summary>
        [HttpGet("admin/stats")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetBlogPostStats()
        {
            try
            {
                var result = await _blogPostService.GetBlogPostStatsAsync();
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog post stats");
                return StatusCode(500, new { message = "Failed to get blog post stats" });
            }
        }

        /// <summary>
        /// Get blog posts by date range (Admin/Manager only)
        /// </summary>
        [HttpGet("admin/date-range")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetBlogPostsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _blogPostService.GetPostsByDateRangeAsync(startDate, endDate);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog posts by date range");
                return StatusCode(500, new { message = "Failed to get blog posts by date range" });
            }
        }

        /// <summary>
        /// Bulk publish blog posts (Admin/Manager only)
        /// </summary>
        [HttpPatch("admin/bulk-publish")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> BulkPublishBlogPosts([FromBody] int[] postIds)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var results = new List<object>();
                foreach (var postId in postIds)
                {
                    var result = await _blogPostService.PublishBlogPostAsync(postId, userId.Value);
                    results.Add(new { PostId = postId, Success = result.IsSuccess, Message = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    message = "Bulk publish operation completed",
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk publishing blog posts");
                return StatusCode(500, new { message = "Failed to bulk publish blog posts" });
            }
        }

        /// <summary>
        /// Bulk unpublish blog posts (Admin/Manager only)
        /// </summary>
        [HttpPatch("admin/bulk-unpublish")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> BulkUnpublishBlogPosts([FromBody] int[] postIds)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var results = new List<object>();
                foreach (var postId in postIds)
                {
                    var result = await _blogPostService.UnpublishBlogPostAsync(postId, userId.Value);
                    results.Add(new { PostId = postId, Success = result.IsSuccess, Message = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    message = "Bulk unpublish operation completed",
                    data = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk unpublishing blog posts");
                return StatusCode(500, new { message = "Failed to bulk unpublish blog posts" });
            }
        }

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}