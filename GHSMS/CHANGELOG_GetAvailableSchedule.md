# Changelog - GetAvailableSchedule API Enhancement

## Ngày thay đổi: [Current Date]

## Mô tả thay đổi
Cập nhật logic cho API `GetAvailableSchedule` để xử lý trường hợp không có schedule nào trong ngày được yêu cầu.

## Vấn đề cũ
- Khi không có schedule nào trong khoảng thời gian được yêu cầu, API trả về danh sách rỗng
- Người dùng không thể thấy các consultant có sẵn để đặt lịch

## Giải pháp mới
- **Khi có schedule**: Trả về danh sách schedule thực tế như trước
- **Khi không có schedule (schedule = 0)**: Trả về tất cả user có role "consultant"

## Files đã thay đổi

### 1. Service/Services/AppointmentService.cs
**Method:** `GetAvailableSchedulesAsync(DateTime startDate, DateTime endDate)`

**Thay đổi:**
```csharp
// Logic mới được thêm vào
if (!schedules.Any())
{
    var consultantUsers = await _userRepo.GetUsersByRoleAsync("consultant");
    
    foreach (var user in consultantUsers)
    {
        availableSchedules.Add(new AvailableScheduleDto
        {
            ScheduleId = 0, // No specific schedule
            ConsultantId = user.UserId,
            ConsultantName = $"{user.FirstName} {user.LastName}",
            ConsultantSpecialization = user.ConsultantProfile?.Specialization ?? "General",
            ConsultantExperience = user.ConsultantProfile?.Experience ?? "Not specified",
            StartTime = startDate,
            EndTime = endDate,
            IsAvailable = true
        });
    }
}
```

### 2. Repository/Repositories/UserRepo.cs
**Method:** `GetUsersByRoleAsync(string roleName)`

**Thay đổi:**
```csharp
// Thêm Include ConsultantProfile để load thông tin consultant
return await _dbSet
    .Include(u => u.Roles)
    .Include(u => u.ConsultantProfile)  // <- Dòng này được thêm
    .Where(u => u.Roles.Any(r => r.RoleName == roleName))
    .ToListAsync();
```

## Lợi ích của thay đổi

1. **Trải nghiệm người dùng tốt hơn**: 
   - Luôn hiển thị danh sách consultant có sẵn
   - Không bao giờ trả về danh sách rỗng

2. **Linh hoạt trong đặt lịch**:
   - Cho phép đặt lịch với consultant ngay cả khi họ chưa tạo schedule cụ thể
   - Hỗ trợ đặt lịch linh hoạt

3. **Phân biệt rõ ràng**:
   - `ScheduleId = 0`: Không có schedule cụ thể
   - `ScheduleId > 0`: Có schedule thực tế
   - `IsAvailable = true`: Mặc định available cho consultant

## Cách sử dụng

### Frontend có thể xử lý như sau:
```javascript
// Kiểm tra loại schedule
if (schedule.ScheduleId === 0) {
    // Đây là consultant không có schedule cụ thể
    // Có thể cho phép user chọn thời gian tự do
} else {
    // Đây là schedule thực tế với thời gian cố định
    // Sử dụng StartTime và EndTime từ schedule
}
```

## Testing
- Test case 1: Có schedule trong khoảng thời gian → Trả về schedule thực tế
- Test case 2: Không có schedule → Trả về tất cả consultant với ScheduleId = 0
- Test case 3: Một số consultant có schedule, một số không → Trả về cả hai loại

## Notes
- Role name phải chính xác là "consultant" (lowercase)
- ConsultantProfile có thể null, đã xử lý với null-coalescing operator
- StartTime và EndTime sử dụng từ request parameters khi không có schedule cụ thể