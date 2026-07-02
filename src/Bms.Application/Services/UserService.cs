using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bms.Application.DTOs;
using Bms.Application.Interfaces;
using Bms.Domain.Entities;
using Bms.Domain.Enums;
using Bms.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Bms.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserDto?> LoginAsync(string usernameOrEmail, string password)
    {
        _logger.LogInformation("Attempting login for user: {UsernameOrEmail}", usernameOrEmail);
        try
        {
            var users = await _unitOfWork.GetRepository<User>().FindAsync(u => 
                (u.Username.ToLower() == usernameOrEmail.ToLower() || u.Email.ToLower() == usernameOrEmail.ToLower()) 
                && u.IsActive);

            var user = users.FirstOrDefault();
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found or inactive. User: {UsernameOrEmail}", usernameOrEmail);
                return null;
            }

            bool isValid = _passwordHasher.VerifyPassword(password, user.PasswordHash);
            if (!isValid)
            {
                _logger.LogWarning("Login failed: Incorrect password for user: {UsernameOrEmail}", usernameOrEmail);
                return null;
            }

            _logger.LogInformation("User {UsernameOrEmail} logged in successfully", usernameOrEmail);
            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for user: {UsernameOrEmail}", usernameOrEmail);
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        _logger.LogInformation("Fetching all users");
        try
        {
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();
            var departments = await _unitOfWork.GetRepository<Department>().GetAllAsync();
            var deptDict = departments.ToDictionary(d => d.Id, d => d.Name);

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.DepartmentId.HasValue && deptDict.TryGetValue(u.DepartmentId.Value, out var name) ? name : "None",
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            throw;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching user with ID: {UserId}", id);
        try
        {
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(id);
            if (user == null) return null;

            var dto = MapToDto(user);
            if (user.DepartmentId.HasValue)
            {
                var dept = await _unitOfWork.GetRepository<Department>().GetByIdAsync(user.DepartmentId.Value);
                if (dept != null) dto.DepartmentName = dept.Name;
            }
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
            throw;
        }
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        _logger.LogInformation("Creating new user: {Username}", createUserDto.Username);
        try
        {
            // Business rule validations
            var existingUsername = await _unitOfWork.GetRepository<User>().FindAsync(u => u.Username.ToLower() == createUserDto.Username.ToLower());
            if (existingUsername.Any())
            {
                throw new InvalidOperationException($"Username '{createUserDto.Username}' is already taken.");
            }

            var existingEmail = await _unitOfWork.GetRepository<User>().FindAsync(u => u.Email.ToLower() == createUserDto.Email.ToLower());
            if (existingEmail.Any())
            {
                throw new InvalidOperationException($"Email '{createUserDto.Email}' is already registered.");
            }

            if (createUserDto.DepartmentId.HasValue)
            {
                var dept = await _unitOfWork.GetRepository<Department>().GetByIdAsync(createUserDto.DepartmentId.Value);
                if (dept == null)
                {
                    throw new InvalidOperationException("Assigned department does not exist.");
                }
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                PasswordHash = _passwordHasher.HashPassword(createUserDto.Password),
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Role = createUserDto.Role,
                DepartmentId = createUserDto.DepartmentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created user: {Username} (ID: {UserId})", user.Username, user.Id);
            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", createUserDto.Username);
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(UpdateUserDto updateUserDto)
    {
        _logger.LogInformation("Updating user: {UserId}", updateUserDto.Id);
        try
        {
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(updateUserDto.Id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // Check email uniqueness
            var existingEmail = await _unitOfWork.GetRepository<User>().FindAsync(u => u.Email.ToLower() == updateUserDto.Email.ToLower() && u.Id != updateUserDto.Id);
            if (existingEmail.Any())
            {
                throw new InvalidOperationException($"Email '{updateUserDto.Email}' is already registered by another user.");
            }

            if (updateUserDto.DepartmentId.HasValue)
            {
                var dept = await _unitOfWork.GetRepository<Department>().GetByIdAsync(updateUserDto.DepartmentId.Value);
                if (dept == null)
                {
                    throw new InvalidOperationException("Assigned department does not exist.");
                }
            }

            user.Email = updateUserDto.Email;
            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.Role = updateUserDto.Role;
            user.DepartmentId = updateUserDto.DepartmentId;
            user.IsActive = updateUserDto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(updateUserDto.Password);
            }

            _unitOfWork.GetRepository<User>().Update(user);
            int affected = await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated user: {UserId}", user.Id);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", updateUserDto.Id);
            throw;
        }
    }

    public async Task<bool> ToggleUserStatusAsync(Guid id)
    {
        _logger.LogInformation("Toggling active status for user: {UserId}", id);
        try
        {
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.GetRepository<User>().Update(user);
            int affected = await _unitOfWork.SaveChangesAsync();
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for user: {UserId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
    {
        _logger.LogInformation("Fetching all departments");
        try
        {
            var depts = await _unitOfWork.GetRepository<Department>().GetAllAsync();
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();

            var counts = users.Where(u => u.DepartmentId.HasValue)
                              .GroupBy(u => u.DepartmentId!.Value)
                              .ToDictionary(g => g.Key, g => g.Count());

            return depts.Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                EmployeeCount = counts.TryGetValue(d.Id, out int count) ? count : 0,
                CreatedAt = d.CreatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching departments");
            throw;
        }
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto)
    {
        _logger.LogInformation("Creating new department: {DeptName}", createDepartmentDto.Name);
        try
        {
            var existing = await _unitOfWork.GetRepository<Department>().FindAsync(d => d.Name.ToLower() == createDepartmentDto.Name.ToLower());
            if (existing.Any())
            {
                throw new InvalidOperationException($"Department '{createDepartmentDto.Name}' already exists.");
            }

            var dept = new Department
            {
                Id = Guid.NewGuid(),
                Name = createDepartmentDto.Name,
                Description = createDepartmentDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<Department>().AddAsync(dept);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created department: {DeptName} (ID: {DeptId})", dept.Name, dept.Id);
            return new DepartmentDto
            {
                Id = dept.Id,
                Name = dept.Name,
                Description = dept.Description,
                EmployeeCount = 0,
                CreatedAt = dept.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department: {DeptName}", createDepartmentDto.Name);
            throw;
        }
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.Name ?? "None",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
