using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AttendanceMaSys.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<(Result Result, string UserId)> CreateUserWithRoleAsync(string userName, string password, string role)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded && !string.IsNullOrEmpty(role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<(Result Result, string UserId, string Role)> ValidateUserCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);
        if (user == null)
        {
            return (Result.Failure(["Invalid credentials."]), string.Empty, string.Empty);
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            return (Result.Failure(["Tài khoản này đã bị khóa / vô hiệu hóa do nhân viên đã nghỉ việc."]), string.Empty, string.Empty);
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, password);
        if (!validPassword)
        {
            return (Result.Failure(["Invalid credentials."]), string.Empty, string.Empty);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";

        return (Result.Success(), user.Id, role);
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);
        return user != null;
    }

    public async Task<Result> UpdateUserRoleAsync(string emailOrPhone, string newRole)
    {
        var user = await FindUserByEmailOrPhoneAsync(emailOrPhone);
        if (user == null) return Result.Failure(["Không tìm thấy tài khoản người dùng."]);

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        var result = await _userManager.AddToRoleAsync(user, newRole);
        return result.ToApplicationResult();
    }

    public async Task<Result> DeactivateUserAsync(string emailOrPhone)
    {
        var user = await FindUserByEmailOrPhoneAsync(emailOrPhone);
        if (user == null) return Result.Failure(["Không tìm thấy tài khoản người dùng."]);

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.LockoutEnabled = true;

        var result = await _userManager.UpdateAsync(user);
        return result.ToApplicationResult();
    }

    private async Task<ApplicationUser?> FindUserByEmailOrPhoneAsync(string emailOrPhone)
    {
        return await _userManager.FindByEmailAsync(emailOrPhone) ??
               await _userManager.FindByNameAsync(emailOrPhone) ??
               await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == emailOrPhone);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
}
