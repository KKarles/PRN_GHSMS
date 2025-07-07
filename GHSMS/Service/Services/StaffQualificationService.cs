using Repository.Models;
using Repository.Repositories;
using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public class StaffQualificationService : IStaffQualificationService
    {
        private readonly IConsultantProfileRepo _consultantProfileRepo;
        private readonly IUserRepo _userRepo;

        public StaffQualificationService(IConsultantProfileRepo consultantProfileRepo, IUserRepo userRepo)
        {
            _consultantProfileRepo = consultantProfileRepo;
            _userRepo = userRepo;
        }

        public async Task<ResultModel> GetStaffQualificationAsync(int staffId)
        {
            try
            {
                // Verify user exists and is staff/consultant
                var user = await _userRepo.GetUserWithRolesAsync(staffId);
                if (user == null)
                {
                    return ResultModel.NotFound("Staff member not found");
                }

                if (!user.Roles.Any(r => r.RoleName == "Staff" || r.RoleName == "Consultant"))
                {
                    return ResultModel.BadRequest("User is not a staff member or consultant");
                }

                var profile = await _consultantProfileRepo.GetByConsultantIdWithUserAsync(staffId);
                var staffQualification = MapToStaffQualificationDto(user, profile);

                return ResultModel.Success(staffQualification);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get staff qualification: {ex.Message}");
            }
        }

        public async Task<ResultModel> CreateStaffQualificationAsync(int staffId, CreateStaffQualificationDto createDto)
        {
            try
            {
                // Verify user exists and is staff/consultant
                var user = await _userRepo.GetUserWithRolesAsync(staffId);
                if (user == null)
                {
                    return ResultModel.NotFound("Staff member not found");
                }

                if (!user.Roles.Any(r => r.RoleName == "Staff" || r.RoleName == "Consultant"))
                {
                    return ResultModel.BadRequest("User is not a staff member or consultant");
                }

                // Check if profile already exists
                var existingProfile = await _consultantProfileRepo.GetByConsultantIdAsync(staffId);
                if (existingProfile != null)
                {
                    return ResultModel.Conflict("Staff qualification profile already exists. Use update instead.");
                }

                // Create new consultant profile
                var profile = new ConsultantProfile
                {
                    ConsultantId = staffId,
                    Qualifications = createDto.Qualifications.Trim(),
                    Experience = createDto.Experience?.Trim(),
                    Specialization = createDto.Specialization?.Trim()
                };

                var createdProfile = await _consultantProfileRepo.CreateAsync(profile);
                var staffQualification = MapToStaffQualificationDto(user, createdProfile);

                return ResultModel.Created(staffQualification, "Staff qualification created successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to create staff qualification: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateStaffQualificationAsync(int staffId, UpdateStaffQualificationDto updateDto)
        {
            try
            {
                // Verify user exists and is staff/consultant
                var user = await _userRepo.GetUserWithRolesAsync(staffId);
                if (user == null)
                {
                    return ResultModel.NotFound("Staff member not found");
                }

                if (!user.Roles.Any(r => r.RoleName == "Staff" || r.RoleName == "Consultant"))
                {
                    return ResultModel.BadRequest("User is not a staff member or consultant");
                }

                var profile = await _consultantProfileRepo.GetByConsultantIdAsync(staffId);
                if (profile == null)
                {
                    return ResultModel.NotFound("Staff qualification profile not found. Create one first.");
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateDto.Qualifications))
                    profile.Qualifications = updateDto.Qualifications.Trim();

                if (updateDto.Experience != null)
                    profile.Experience = string.IsNullOrWhiteSpace(updateDto.Experience) ? null : updateDto.Experience.Trim();

                if (updateDto.Specialization != null)
                    profile.Specialization = string.IsNullOrWhiteSpace(updateDto.Specialization) ? null : updateDto.Specialization.Trim();

                await _consultantProfileRepo.UpdateAsync(profile);

                // Return updated profile
                var updatedProfile = await _consultantProfileRepo.GetByConsultantIdWithUserAsync(staffId);
                var staffQualification = MapToStaffQualificationDto(user, updatedProfile);

                return ResultModel.Success(staffQualification, "Staff qualification updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update staff qualification: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteStaffQualificationAsync(int staffId)
        {
            try
            {
                var profile = await _consultantProfileRepo.GetByConsultantIdAsync(staffId);
                if (profile == null)
                {
                    return ResultModel.NotFound("Staff qualification profile not found");
                }

                await _consultantProfileRepo.RemoveAsync(profile);
                return ResultModel.Success(null, "Staff qualification deleted successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to delete staff qualification: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetAllStaffQualificationsAsync()
        {
            try
            {
                var profiles = await _consultantProfileRepo.GetAllWithUserDetailsAsync();
                var staffQualifications = profiles.Select(p => MapToStaffQualificationDto(p.Consultant, p)).ToList();

                // Also get staff without qualifications
                var allStaff = await _userRepo.GetUsersByRoleAsync("Staff");
                var consultants = await _userRepo.GetUsersByRoleAsync("Consultant");
                var allStaffAndConsultants = allStaff.Concat(consultants).ToList();

                foreach (var staff in allStaffAndConsultants)
                {
                    if (!staffQualifications.Any(sq => sq.ConsultantId == staff.UserId))
                    {
                        staffQualifications.Add(MapToStaffQualificationDto(staff, null));
                    }
                }

                return ResultModel.Success(staffQualifications.OrderBy(sq => sq.LastName).ThenBy(sq => sq.FirstName).ToList());
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get all staff qualifications: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetStaffBySpecializationAsync(string specialization)
        {
            try
            {
                var profiles = await _consultantProfileRepo.GetBySpecializationAsync(specialization);
                var staffQualifications = profiles.Select(p => MapToStaffQualificationDto(p.Consultant, p)).ToList();

                return ResultModel.Success(staffQualifications);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get staff by specialization: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetStaffWithoutQualificationsAsync()
        {
            try
            {
                var allStaff = await _userRepo.GetUsersByRoleAsync("Staff");
                var consultants = await _userRepo.GetUsersByRoleAsync("Consultant");
                var allStaffAndConsultants = allStaff.Concat(consultants).ToList();

                var staffWithoutQualifications = new List<StaffQualificationDto>();

                foreach (var staff in allStaffAndConsultants)
                {
                    var hasProfile = await _consultantProfileRepo.ExistsByConsultantIdAsync(staff.UserId);
                    if (!hasProfile)
                    {
                        staffWithoutQualifications.Add(MapToStaffQualificationDto(staff, null));
                    }
                }

                return ResultModel.Success(staffWithoutQualifications.OrderBy(sq => sq.LastName).ThenBy(sq => sq.FirstName).ToList());
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get staff without qualifications: {ex.Message}");
            }
        }

        private StaffQualificationDto MapToStaffQualificationDto(User user, ConsultantProfile? profile)
        {
            return new StaffQualificationDto
            {
                ConsultantId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Qualifications = profile?.Qualifications,
                Experience = profile?.Experience,
                Specialization = profile?.Specialization,
                HasProfile = profile != null
            };
        }
    }
}