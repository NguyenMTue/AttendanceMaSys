using MindVaultAI.Application.Auth.DTOs;
using MindVaultAI.Application.Common.Interfaces;

namespace MindVaultAI.Application.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IEmployeeRepository _employeeRepository;

    public LoginCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        IEmployeeRepository employeeRepository)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _employeeRepository = employeeRepository;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (result, userId, role) = await _identityService.ValidateUserCredentialsAsync(request.Email, request.Password);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var employee = await _employeeRepository.GetByEmailAsync(request.Email, cancellationToken);
        var departmentStr = employee?.Department.ToString() ?? role;
        var employeeId = employee?.Id;

        var token = _tokenService.GenerateToken(userId, request.Email, role, departmentStr, employeeId);

        return new LoginResponseDto
        {
            Token = token,
            UserId = userId,
            Email = request.Email,
            Role = role,
            Department = departmentStr,
            EmployeeId = employeeId
        };
    }
}
