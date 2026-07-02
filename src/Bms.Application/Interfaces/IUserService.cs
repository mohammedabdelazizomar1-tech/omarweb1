using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bms.Application.DTOs;

namespace Bms.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> LoginAsync(string usernameOrEmail, string password);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<bool> UpdateUserAsync(UpdateUserDto updateUserDto);
    Task<bool> ToggleUserStatusAsync(Guid id);
    Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto);
}
