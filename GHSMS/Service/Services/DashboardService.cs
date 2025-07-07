//using Repository.Repositories;
//using Service.DTOs;
//using Service.Models;

//namespace Service.Services
//{
//    public class DashboardService : IDashboardService
//    {
//        private readonly ITestBookingRepo _testBookingRepo;
//        private readonly IUserRepo _userRepo;
//        private readonly IServiceRepo _serviceRepo;

//        public DashboardService(ITestBookingRepo testBookingRepo, IUserRepo userRepo, IServiceRepo serviceRepo)
//        {
//            _testBookingRepo = testBookingRepo;
//            _userRepo = userRepo;
//            _serviceRepo = serviceRepo;
//        }

//        public async Task<ResultModel> GetDashboardStatsAsync()
//        {
//            try
//            {
//                var revenueStats = await GetRevenueStatsInternalAsync();
//                var userStats = await GetUserStatsInternalAsync();
//                var bookingStats = await GetBookingStatsInternalAsync();
//                var serviceStats = await GetServiceStatsInternalAsync();

//                var dashboardStats = new DashboardStatsDto
//                {
//                    Revenue = revenueStats,
//                    Users = userStats,
//                    Bookings = bookingStats,
//                    Services = serviceStats
//                };

//                return ResultModel.Success(dashboardStats);
//            }
//            catch (Exception ex)
//            {
//                return ResultModel.InternalServerError($"Failed to get dashboard stats: {ex.Message}");
//            }
//        }

//        public async Task<ResultModel> GetRevenueStatsAsync()
//        {
//            try
//            {
//                var revenueStats = await GetRevenueStatsInternalAsync();
//                return ResultModel.Success(revenueStats);
//            }
//            catch (Exception ex)
//            {
//                return ResultModel.InternalServerError($"Failed to get revenue stats: {ex.Message}");
//            }
//        }

//        public async Task<ResultModel> GetUserStatsAsync()
//        {
//            try
//            {
//                var userStats = await GetUserStatsInternalAsync();
//                return ResultModel.Success(userStats);
//            }
//            catch (Exception ex)
//            {
//                return ResultModel.InternalServerError($"Failed to get user stats: {ex.Message}");
//            }
//        }

//        public async Task<ResultModel> GetBookingStatsAsync()
//        {
//            try
//            {
//                var bookingStats = await GetBookingStatsInternalAsync();
//                return ResultModel.Success(bookingStats);
//            }
//            catch (Exception ex)
//            {
//                return ResultModel.InternalServerError($"Failed to get booking stats: {ex.Message}");
//            }
//        }

//        public async Task<ResultModel> GetServiceStatsAsync()
//        {
//            try
//            {
//                var serviceStats = await GetServiceStatsInternalAsync();
//                return ResultModel.Success(serviceStats);
//            }
//            catch (Exception ex)
//            {
//                return ResultModel.InternalServerError($"Failed to get service stats: {ex.Message}");
//            }
//        }

//        public async Task<ResultModel> GetMonthlyRevenueAsync(int year)
//        {
//            try
//            {
//                var startDate = new DateTime(year, 1, 1);
//                var endDate = new DateTime(year, 12, 31);
//                var bookings = await _testBookingRepo.GetBookingsByDateRangeAsync(startDate, endDate);
                
//                var monthlyRevenue = bookings
//                    .Where(b => b.IsPaid)
//                    .GroupBy(b => new { Year = b.BookedAt?.Year ?? year, Month = b.BookedAt?.Month ?? 1 })
//                    .Select(g => new MonthlyRevenueDto
//                    {
//                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
//                        Revenue = g.Sum(b => b.Service?.Price ?? 0),
//                        BookingCount = g.Count()
//                    })
//                    .OrderBy(m => m.Month)
//                    .ToList();

//                return ResultModel.Success(monthlyRevenue);
//            }
//            catch (Exception ex)
//            {
//                return ResultModel.InternalServerError($"Failed to get monthly revenue: {ex.Message}");
//            }
//        }

//        public async Task<ResultModel> GetRevenueByServiceAsync()
//        {
//            try
//            {
//                var bookings = await _testBookingRepo.GetPaidBookingsAsync();
                
//                var revenueByService = bookings
//                    .GroupBy(b => new { b.ServiceId, ServiceName = b.Service?.ServiceName ?? "Unknown" })
//                    .Select(g => new ServiceRevenueDto
//                    {
//                        ServiceName = g.Key.ServiceName,
//                        Revenue = g.Sum(b => b.Service?.Price ?? 0),
//                        BookingCount = g.Count(),
//                        AveragePrice = g.Average(b => b.Service?.Price ?? 0)
//                    })
//                    .OrderByDescending(s => s.Revenue)
//                    .ToList();

//                return ResultModel.Success(revenueByService);
//            }
//            catch (Exception ex)
//            {
//                return ResultModel.InternalServerError($"Failed to get revenue by service: {ex.Message}");
//            }
//        }

//        public async Task<ResultModel> GetRegistrationTrendsAsync(int days = 30)
//        {
//            try
//            {
//                var startDate = DateTime.UtcNow.AddDays(-days);
//                var users = await _userRepo.GetAllAsync();
                
//                var registrationTrends = users
//                    .Where(u => u.CreatedAt >= startDate)
//                    .GroupBy(u => u.CreatedAt?.Date ?? DateTime.Today)
//                    .Select(g => new DailyRegistrationDto
//                    {
//                        Date = g.Key,
//                        RegistrationCount = g.Count()
//                    })
//                    .OrderBy(r => r.Date)
//                    .ToList();

//                return ResultModel.Success(registrationTrends);
//            }
//            catch (Exception ex)
//            {
//                return ResultModel.InternalServerError($"Failed to get registration trends: {ex.Message}");
//            }
//        }

//        public async Task<ResultModel> GetPopularServicesAsync(int limit = 10)
//        {
//            try
//            {
//                var bookings = await _testBookingRepo.GetBookingsWithServiceAsync();
                
//                var popularServices = bookings
//                    .GroupBy(b => new { b.ServiceId, ServiceName = b.Service?.ServiceName ?? "Unknown", Price = b.Service?.Price ?? 0 })
//                    .Select(g => new PopularServiceDto
//                    {
//                        ServiceName = g.Key.ServiceName,
//                        BookingCount = g.Count(),
//                        Revenue = g.Sum(b => b.IsPaid ? g.Key.Price : 0),
//                        Price = g.Key.Price
//                    })
//                    .OrderByDescending(s => s.BookingCount)
//                    .Take(limit)
//                    .ToList();

//                return ResultModel.Success(popularServices);
//            }
//            catch (Exception ex)
//            {
//                return ResultModel.InternalServerError($"Failed to get popular services: {ex.Message}");
//            }
//        }

//        private async Task<RevenueStatsDto> GetRevenueStatsInternalAsync()
//        {
//            var totalRevenue = await _testBookingRepo.GetTotalRevenueAsync();
//            var now = DateTime.UtcNow;
            
//            // Monthly revenue
//            var monthStart = new DateTime(now.Year, now.Month, 1);
//            var monthlyBookings = await _testBookingRepo.GetBookingsByDateRangeAsync(monthStart, now);
//            var monthlyRevenue = monthlyBookings.Where(b => b.IsPaid).Sum(b => b.Service?.Price ?? 0);

//            // Weekly revenue
//            var weekStart = now.AddDays(-(int)now.DayOfWeek);
//            var weeklyBookings = await _testBookingRepo.GetBookingsByDateRangeAsync(weekStart, now);
//            var weeklyRevenue = weeklyBookings.Where(b => b.IsPaid).Sum(b => b.Service?.Price ?? 0);

//            // Daily revenue
//            var dayStart = now.Date;
//            var dailyBookings = await _testBookingRepo.GetBookingsByDateRangeAsync(dayStart, now);
//            var dailyRevenue = dailyBookings.Where(b => b.IsPaid).Sum(b => b.Service?.Price ?? 0);

//            return new RevenueStatsDto
//            {
//                TotalRevenue = totalRevenue,
//                MonthlyRevenue = monthlyRevenue,
//                WeeklyRevenue = weeklyRevenue,
//                DailyRevenue = dailyRevenue
//            };
//        }

//        private async Task<UserStatsDto> GetUserStatsInternalAsync()
//        {
//            var allUsers = await _userRepo.GetAllAsync(u => u.Roles);
//            var now = DateTime.UtcNow;
            
//            var monthStart = new DateTime(now.Year, now.Month, 1);
//            var weekStart = now.AddDays(-(int)now.DayOfWeek);

//            return new UserStatsDto
//            {
//                TotalUsers = allUsers.Count(),
//                NewUsersThisMonth = allUsers.Count(u => u.CreatedAt >= monthStart),
//                NewUsersThisWeek = allUsers.Count(u => u.CreatedAt >= weekStart),
//                ActiveUsers = allUsers.Count(u => u.UpdatedAt >= now.AddDays(-30)), // Active in last 30 days
//                UsersByRole = allUsers
//                    .SelectMany(u => u.Roles)
//                    .GroupBy(r => r.RoleName)
//                    .ToDictionary(g => g.Key, g => g.Count())
//            };
//        }

//        private async Task<BookingStatsDto> GetBookingStatsInternalAsync()
//        {
//            var allBookings = await _testBookingRepo.GetBookingsWithServiceAsync();
//            var totalRevenue = await _testBookingRepo.GetTotalRevenueAsync();

//            return new BookingStatsDto
//            {
//                TotalBookings = allBookings.Count(),
//                PaidBookings = allBookings.Count(b => b.IsPaid),
//                TotalRevenue = totalRevenue,
//                BookingsByStatus = allBookings
//                    .GroupBy(b => b.BookingStatus)
//                    .ToDictionary(g => g.Key, g => g.Count()),
//                BookingsByService = allBookings
//                    .GroupBy(b => b.Service?.ServiceName ?? "Unknown")
//                    .ToDictionary(g => g.Key, g => g.Count())
//            };
//        }

//        private async Task<ServiceStatsDto> GetServiceStatsInternalAsync()
//        {
//            var allServices = await _serviceRepo.GetAllAsync();
//            var bookings = await _testBookingRepo.GetBookingsWithServiceAsync();

//            var popularServices = bookings
//                .GroupBy(b => new { b.ServiceId, ServiceName = b.Service?.ServiceName ?? "Unknown", Price = b.Service?.Price ?? 0 })
//                .Select(g => new PopularServiceDto
//                {
//                    ServiceName = g.Key.ServiceName,
//                    BookingCount = g.Count(),
//                    Revenue = g.Sum(b => b.IsPaid ? g.Key.Price : 0),
//                    Price = g.Key.Price
//                })
//                .OrderByDescending(s => s.BookingCount)
//                .Take(5)
//                .ToList();

//            return new ServiceStatsDto
//            {
//                TotalServices = allServices.Count(),
//                PopularServices = popularServices,
//                ServicesByType = allServices
//                    .GroupBy(s => s.ServiceType)
//                    .ToDictionary(g => g.Key, g => g.Count())
//            };
//        }

//        public Task<ResultModel> GetAdminDashboardStatsAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ResultModel> GetManagerDashboardStatsAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ResultModel> GetCustomerDashboardAsync(int customerId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ResultModel> GetStaffDashboardStatsAsync()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}