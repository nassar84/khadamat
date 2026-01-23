using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;

namespace Khadamat.BlazorUI.Services.Admin;

public interface IAdminService
{
    Task<AdminStatsDto?> GetStats();
    Task<List<UserDto>> GetAllUsers();
    Task<UserDto?> CreateUser(CreateUserDto dto);
    Task ToggleUserStatus(string id);
    Task DeleteUser(string id);
    Task ApproveService(int id);
    Task RejectService(int id);
    
    Task<List<PendingProviderDto>> GetPendingProviders();
    Task ApproveProvider(int id);
    Task RejectProvider(int id);
    
    Task UpdateUser(string id, UserDto dto);
    Task UpdateUserRole(string id, string role);
    Task ChangePassword(ChangePasswordDto dto);
}
