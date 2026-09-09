namespace MindVaultAI.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(string userId, string email, string role, string? department, Guid? employeeId);
}
