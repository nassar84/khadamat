using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Khadamat.Application.Interfaces;
using Khadamat.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Khadamat.Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Dictionary<string, (string Name, string Avatar)>> GetUsersBasicInfoAsync(IEnumerable<string> userIds)
    {
        var distinctIds = userIds.Distinct().ToList();
        var users = await _userManager.Users
            .Where(u => distinctIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.ProfileImageUrl })
            .ToListAsync();

        return users.ToDictionary(
            u => u.Id, 
            u => (Name: u.FullName ?? "مستخدم", Avatar: u.ProfileImageUrl ?? "")
        );
    }
}
