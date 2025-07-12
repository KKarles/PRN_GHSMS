using Microsoft.EntityFrameworkCore;
using Repository.Repositories;
using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public class UserLookupService : IUserLookupService
    {
        private readonly IUserRepo _userRepo;

        public UserLookupService(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<ResultModel> GetAllUsersAsync(string? role = null, string? search = null, int page = 1, int limit = 10)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (limit < 1) limit = 10;
                if (limit > 100) limit = 100; // Prevent excessive data retrieval

                // Get queryable for users with roles
                var query = _userRepo.GetQueryable()
                    .Include(u => u.Roles)
                    .AsQueryable();

                // Apply role filter
                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Roles.Any(r => r.RoleName == role));
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(u => 
                        u.FirstName.ToLower().Contains(searchLower) ||
                        u.LastName.ToLower().Contains(searchLower) ||
                        u.Email.ToLower().Contains(searchLower) ||
                        (u.PhoneNumber != null && u.PhoneNumber.Contains(search))
                    );
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / limit);

                // Apply pagination
                var users = await query
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Map to DTOs
                var userDtos = users.Select(u => new UserLookupDto
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    RegisteredAt = u.CreatedAt,
                    Roles = u.Roles.Select(r => r.RoleName).ToList(),
                    LastLoginAt = null, // Would need to track this in a real system
                    IsActive = true // Would need to implement user status tracking
                }).ToList();

                var result = new PaginatedUserListDto
                {
                    Pagination = new PaginationDto
                    {
                        CurrentPage = page,
                        TotalPages = totalPages,
                        PageSize = limit,
                        TotalCount = totalCount,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    },
                    Data = userDtos
                };

                return ResultModel.Success(result);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get users: {ex.Message}");
            }
        }
    }
}