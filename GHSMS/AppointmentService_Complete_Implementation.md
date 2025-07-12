# AppointmentService - Complete Implementation

## Overview
Đã implement hoàn chỉnh tất cả 12 methods trong AppointmentService với đầy đủ validation, error handling và business logic.

## ✅ Implemented Methods

### 1. **CreateAppointmentAsync** 
```csharp
Task<ResultModel> CreateAppointmentAsync(int customerId, CreateAppointmentDto createAppointmentDto)
```
**Features:**
- Flexible booking (với hoặc không chỉ định consultant)
- Tự động tạo schedule nếu cần thiết
- Conflict detection
- Role validation cho consultant
- Auto-generate meeting URL

### 2. **GetAppointmentByIdAsync**
```csharp
Task<ResultModel> GetAppointmentByIdAsync(int appointmentId)
```
**Features:**
- Lấy chi tiết appointment theo ID
- Include đầy đủ thông tin customer, consultant, schedule
- Return AppointmentDto với đầy đủ thông tin

### 3. **GetCustomerAppointmentsAsync**
```csharp
Task<ResultModel> GetCustomerAppointmentsAsync(int customerId)
```
**Features:**
- Lấy tất cả appointments của customer
- Sắp xếp theo thời gian (mới nhất trước)
- Include consultant và schedule information

### 4. **GetConsultantAppointmentsAsync**
```csharp
Task<ResultModel> GetConsultantAppointmentsAsync(int consultantId)
```
**Features:**
- Lấy tất cả appointments của consultant
- Include customer và schedule information
- Useful cho consultant dashboard

### 5. **GetAllAppointmentsAsync**
```csharp
Task<ResultModel> GetAllAppointmentsAsync()
```
**Features:**
- Admin function để lấy tất cả appointments
- Include đầy đủ thông tin users và schedules
- Useful cho admin dashboard

### 6. **GetAppointmentsByStatusAsync**
```csharp
Task<ResultModel> GetAppointmentsByStatusAsync(string status)
```
**Features:**
- Filter appointments theo status
- Support các status: "Scheduled", "Confirmed", "InProgress", "Completed", "Cancelled"
- Include đầy đủ thông tin

### 7. **UpdateAppointmentStatusAsync**
```csharp
Task<ResultModel> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusDto updateStatusDto)
```
**Features:**
- Validate status transitions (không cho phép invalid transitions)
- Update meeting URL nếu có
- Business logic cho status workflow

**Valid Status Transitions:**
- Scheduled → Confirmed, InProgress, Cancelled
- Confirmed → InProgress, Cancelled  
- InProgress → Completed, Cancelled
- Completed → (No transitions)
- Cancelled → (No transitions)

### 8. **CancelAppointmentAsync**
```csharp
Task<ResultModel> CancelAppointmentAsync(int appointmentId, int userId)
```
**Features:**
- Authorization check (chỉ customer hoặc consultant mới cancel được)
- Validate appointment status (không cancel được nếu đã Completed/Cancelled)
- Update status thành "Cancelled"

### 9. **GetAvailableSchedulesAsync**
```csharp
Task<ResultModel> GetAvailableSchedulesAsync(DateTime startDate, DateTime endDate)
```
**Features:**
- Logic đã được cập nhật trước đó
- Trả về schedules có sẵn hoặc tất cả consultants nếu không có schedule
- Include consultant information và specialization

### 10. **GetConsultantSchedulesAsync**
```csharp
Task<ResultModel> GetConsultantSchedulesAsync(int consultantId, DateTime startDate, DateTime endDate)
```
**Features:**
- Lấy schedules của consultant cụ thể
- Show trạng thái IsBooked cho mỗi slot
- Useful cho booking interface

### 11. **GetUpcomingAppointmentsAsync**
```csharp
Task<ResultModel> GetUpcomingAppointmentsAsync(int userId)
```
**Features:**
- Lấy appointments sắp tới của user (customer hoặc consultant)
- Filter theo thời gian (chỉ future appointments)
- Exclude cancelled appointments
- Sắp xếp theo thời gian

### 12. **GetAppointmentStatsAsync**
```csharp
Task<ResultModel> GetAppointmentStatsAsync()
```
**Features:**
- Dashboard statistics
- Total, Completed, Cancelled, Upcoming counts
- Group by status và consultant
- Useful cho admin analytics

### 13. **GetAppointmentsByDateRangeAsync**
```csharp
Task<ResultModel> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
```
**Features:**
- Filter appointments theo date range
- Include đầy đủ thông tin
- Useful cho reports và calendar views

### 14. **SetMeetingUrlAsync**
```csharp
Task<ResultModel> SetMeetingUrlAsync(int appointmentId, string meetingUrl, int consultantId)
```
**Features:**
- Chỉ consultant được assign mới có thể set meeting URL
- Authorization check
- Update meeting URL cho appointment

## 🔧 Helper Methods

### 1. **MapToAppointmentDto**
```csharp
private async Task<AppointmentDto> MapToAppointmentDto(Appointment appointment)
```
- Convert entity sang DTO
- Handle lazy loading
- Provide fallback values

### 2. **GenerateDefaultMeetingUrl**
```csharp
private string GenerateDefaultMeetingUrl(int consultantId, int customerId)
```
- Generate unique meeting URLs
- Format: `https://meet.ghsms.com/room/{consultantId}-{customerId}-{meetingId}`

### 3. **IsValidStatusTransition**
```csharp
private bool IsValidStatusTransition(string currentStatus, string newStatus)
```
- Validate appointment status workflow
- Prevent invalid status changes

## 🎯 Key Features

### **Comprehensive Error Handling**
- Try-catch blocks cho tất cả methods
- Meaningful error messages
- Proper HTTP status codes

### **Authorization & Validation**
- Role-based access control
- User ownership validation
- Input validation

### **Business Logic**
- Status transition validation
- Conflict detection
- Flexible scheduling

### **Performance Optimization**
- Efficient database queries
- Proper includes để avoid N+1 queries
- Async/await pattern

## 📊 Return Types

### **Success Responses**
- `ResultModel.Success(data)` - 200 OK
- `ResultModel.Created(data, message)` - 201 Created

### **Error Responses**
- `ResultModel.NotFound(message)` - 404 Not Found
- `ResultModel.BadRequest(message)` - 400 Bad Request
- `ResultModel.Forbidden(message)` - 403 Forbidden
- `ResultModel.InternalServerError(message)` - 500 Internal Server Error

## 🔗 Dependencies

### **Repository Dependencies**
- ✅ IAppointmentRepo - Appointment CRUD operations
- ✅ IScheduleRepo - Schedule management
- ✅ IUserRepo - User validation and lookup
- ✅ IConsultantProfileRepo - Consultant information

### **All dependencies properly registered in DI container**

## 🧪 Usage Examples

### **Create Appointment**
```csharp
var createDto = new CreateAppointmentDto
{
    PreferredStartTime = DateTime.UtcNow.AddDays(1),
    PreferredEndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
    PreferredConsultantId = 123 // Optional
};
var result = await appointmentService.CreateAppointmentAsync(customerId, createDto);
```

### **Update Status**
```csharp
var updateDto = new UpdateAppointmentStatusDto
{
    AppointmentStatus = "Confirmed",
    MeetingUrl = "https://zoom.us/j/123456789"
};
var result = await appointmentService.UpdateAppointmentStatusAsync(appointmentId, updateDto);
```

### **Get Statistics**
```csharp
var stats = await appointmentService.GetAppointmentStatsAsync();
// Returns: TotalAppointments, CompletedAppointments, etc.
```

## ✨ Benefits

1. **Complete Functionality**: Tất cả CRUD operations và business logic
2. **Flexible Booking**: Support nhiều scenarios khác nhau
3. **Robust Validation**: Comprehensive input và business validation
4. **Security**: Authorization checks và role validation
5. **Performance**: Optimized database queries
6. **Maintainability**: Clean code structure với proper error handling
7. **Extensibility**: Easy to add new features

## 🚀 Ready for Production
AppointmentService hiện tại đã sẵn sàng cho production với đầy đủ features cần thiết cho một hệ thống booking appointment hoàn chỉnh!