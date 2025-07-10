namespace Service.DTOs
{
    public class UserLookupDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PaginatedUserListDto
    {
        public PaginationDto Pagination { get; set; } = new PaginationDto();
        public List<UserLookupDto> Data { get; set; } = new List<UserLookupDto>();
    }

    public class PaginationDto
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}