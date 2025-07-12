using System.ComponentModel.DataAnnotations;

namespace Service.DTOs
{
    public class BlogPostCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(int.MaxValue)] // Allow maximum length for content with images
        public string Content { get; set; } = string.Empty;

        public bool PublishNow { get; set; } = false;
    }

    public class BlogPostUpdateDto
    {
        [MaxLength(255)]
        public string? Title { get; set; }

        [MaxLength(int.MaxValue)] // Allow maximum length for content with images
        public string? Content { get; set; }

        public bool? IsPublished { get; set; }
    }

    public class BlogPostDto
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsPublished { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class BlogPostSummaryDto
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentPreview { get; set; } = string.Empty;
        public int? AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsPublished { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class BlogPostSearchDto
    {
        public string? SearchTerm { get; set; }
        public int? AuthorId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsPublished { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PaginatedBlogPostsDto
    {
        public IEnumerable<BlogPostSummaryDto> Posts { get; set; } = new List<BlogPostSummaryDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}