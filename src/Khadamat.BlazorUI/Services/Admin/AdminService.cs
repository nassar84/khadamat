using System.Net.Http.Json;
using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;

namespace Khadamat.BlazorUI.Services.Admin;

public class AdminService : IAdminService
{
    private readonly HttpClient _http;

    public AdminService(HttpClient http)
    {
        _http = http;
    }

    public async Task<AdminStatsDto?> GetStats()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<AdminStatsDto>>("api/v1/admin/stats");
        return response?.Data;
    }

    public async Task<List<UserDto>> GetAllUsers()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<UserDto>>>("api/v1/admin/users");
        return response?.Data ?? new List<UserDto>();
    }

    public async Task<UserDto?> CreateUser(CreateUserDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/admin/users", dto);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        return result?.Data;
    }

    public async Task ToggleUserStatus(string id)
    {
        var response = await _http.PostAsync($"api/v1/admin/users/{id}/toggle-status", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUser(string id)
    {
        var response = await _http.DeleteAsync($"api/v1/admin/users/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ApproveService(int id)
    {
        await _http.PostAsync($"api/v1/admin/services/{id}/approve", null);
    }

    public async Task RejectService(int id)
    {
        await _http.PostAsync($"api/v1/admin/services/{id}/reject", null);
    }

    public async Task<List<PendingProviderDto>> GetPendingProviders()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PendingProviderDto>>>("api/v1/admin/providers/pending");
        return response?.Data ?? new List<PendingProviderDto>();
    }

    public async Task ApproveProvider(int id)
    {
        await _http.PostAsync($"api/v1/admin/providers/{id}/approve", null);
    }

    public async Task RejectProvider(int id)
    {
        await _http.PostAsync($"api/v1/admin/providers/{id}/reject", null);
    }

    public async Task UpdateUser(string id, UserDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/admin/users/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateUserRole(string id, string role)
    {
        var response = await _http.PostAsJsonAsync($"api/v1/admin/users/{id}/role", role);
        response.EnsureSuccessStatusCode();
    }

    public async Task ChangePassword(ChangePasswordDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/admin/users/change-password", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateService(int id, ServiceDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/admin/services/{id}", dto);
        response.EnsureSuccessStatusCode();
    }
}
