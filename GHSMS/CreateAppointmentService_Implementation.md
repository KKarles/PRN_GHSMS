# CreateAppointment Service Implementation

## Overview
Đã implement hoàn chỉnh method `CreateAppointmentAsync` trong `AppointmentService` với logic linh hoạt và robust.

## Key Features

### 1. Validation
- ✅ Kiểm tra customer tồn tại
- ✅ Validate thời gian appointment phải trong tương lai
- ✅ Validate EndTime > StartTime
- ✅ Kiểm tra consultant có role "consultant"

### 2. Flexible Scheduling Logic

#### Scenario 1: Có chỉ định consultant cụ thể
```csharp
if (createAppointmentDto.PreferredConsultantId.HasValue)
{
    // 1. Tìm schedule có sẵn của consultant
    // 2. Nếu không có schedule → Tạo schedule mới
    // 3. Validate consultant role
}
```

#### Scenario 2: Không chỉ định consultant (Auto-assign)
```csharp
else
{
    // 1. Tìm schedule có sẵn của bất kỳ consultant nào
    // 2. Nếu không có → Lấy consultant đầu tiên và tạo schedule mới
}
```

### 3. Schedule Creation
- Tự động tạo schedule mới nếu không tìm thấy schedule phù hợp
- Đảm bảo luôn có thể book appointment ngay cả khi consultant chưa tạo schedule

### 4. Conflict Detection
- Kiểm tra appointment trùng lặp trước khi tạo
- Đảm bảo không double-booking

### 5. Meeting URL Generation
- Tự động tạo meeting URL unique cho mỗi appointment
- Format: `https://meet.ghsms.com/room/{consultantId}-{customerId}-{meetingId}`

## Method Signature
```csharp
public async Task<ResultModel> CreateAppointmentAsync(int customerId, CreateAppointmentDto createAppointmentDto)
```

## Input DTO
```csharp
public class CreateAppointmentDto
{
    [Required]
    public DateTime PreferredStartTime { get; set; }
    
    [Required]
    public DateTime PreferredEndTime { get; set; }
    
    public int? PreferredConsultantId { get; set; }  // Optional
    
    public string? Notes { get; set; }               // Optional
}
```

## Output
```csharp
// Success Response
{
    "isSuccess": true,
    "statusCode": 201,
    "message": "Appointment created successfully",
    "data": {
        "appointmentId": 123,
        "customerId": 456,
        "customerName": "John Doe",
        "customerEmail": "john@example.com",
        "consultantId": 789,
        "consultantName": "Dr. Smith",
        "consultantSpecialization": "Gynecology",
        "scheduleId": 101,
        "startTime": "2024-01-15T10:00:00Z",
        "endTime": "2024-01-15T11:00:00Z",
        "appointmentStatus": "Scheduled",
        "meetingUrl": "https://meet.ghsms.com/room/789-456-abc12345",
        "createdAt": "2024-01-10T08:30:00Z"
    }
}
```

## Error Handling

### Common Error Responses
1. **Customer not found** (404)
2. **Appointment time must be in the future** (400)
3. **End time must be after start time** (400)
4. **Specified consultant not found** (400)
5. **Specified user is not a consultant** (400)
6. **No consultants available in the system** (400)
7. **The selected time slot is no longer available** (400)
8. **Internal server error** (500)

## Dependencies Required

### Repository Layer
- ✅ `IUserRepo` - User validation and consultant lookup
- ✅ `IScheduleRepo` - Schedule management
- ✅ `IAppointmentRepo` - Appointment CRUD operations
- ✅ `IConsultantProfileRepo` - Consultant profile information

### All repositories are properly registered in DI container

## Helper Methods Implemented

### 1. MapToAppointmentDto
```csharp
private async Task<AppointmentDto> MapToAppointmentDto(Appointment appointment)
```
- Converts Appointment entity to AppointmentDto
- Handles lazy loading of related entities
- Provides fallback values for null properties

### 2. GenerateDefaultMeetingUrl
```csharp
private string GenerateDefaultMeetingUrl(int consultantId, int customerId)
```
- Creates unique meeting URL for each appointment
- Uses GUID for uniqueness

### 3. IsValidStatusTransition
```csharp
private bool IsValidStatusTransition(string currentStatus, string newStatus)
```
- Validates appointment status transitions
- Prevents invalid status changes

## Usage Example

### Frontend Call
```javascript
const appointmentData = {
    preferredStartTime: "2024-01-15T10:00:00Z",
    preferredEndTime: "2024-01-15T11:00:00Z",
    preferredConsultantId: 789,  // Optional
    notes: "Regular checkup"     // Optional
};

const response = await fetch('/api/appointment', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
    },
    body: JSON.stringify(appointmentData)
});
```

### Controller Usage
```csharp
[HttpPost]
public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto createAppointmentDto)
{
    var userId = GetUserIdFromToken(); // Extract from JWT
    var result = await _appointmentService.CreateAppointmentAsync(userId.Value, createAppointmentDto);
    return StatusCode(result.StatusCode, result);
}
```

## Benefits

1. **Flexibility**: Có thể book với consultant cụ thể hoặc auto-assign
2. **Reliability**: Tự động tạo schedule nếu cần thiết
3. **Validation**: Comprehensive input validation
4. **Error Handling**: Clear error messages
5. **Scalability**: Efficient database queries with proper includes
6. **Security**: Role-based validation for consultants

## Testing Scenarios

1. ✅ Book với consultant có schedule sẵn
2. ✅ Book với consultant không có schedule → Tạo schedule mới
3. ✅ Auto-assign consultant khi không chỉ định
4. ✅ Handle conflict detection
5. ✅ Validate input data
6. ✅ Error handling cho các edge cases