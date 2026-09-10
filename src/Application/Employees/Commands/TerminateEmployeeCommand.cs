using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Domain.Entities;

namespace AttendanceMaSys.Application.Employees.Commands;

public record TerminateEmployeeCommand(Guid EmployeeId, string? Reason = null) : IRequest<bool>;

public class TerminateEmployeeCommandHandler : IRequestHandler<TerminateEmployeeCommand, bool>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IIdentityService _identityService;

    public TerminateEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        IIdentityService identityService)
    {
        _employeeRepository = employeeRepository;
        _identityService = identityService;
    }

    public async Task<bool> Handle(TerminateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            throw new NotFoundException(nameof(Employee), request.EmployeeId.ToString());
        }

        if (!employee.IsActive)
        {
            return true; // Already inactive
        }

        // 1. Deactivate Domain Entity
        employee.IsActive = false;
        await _employeeRepository.UpdateAsync(employee, cancellationToken);

        // 2. Lockout & Revoke Access Rights on Identity Account
        if (!string.IsNullOrEmpty(employee.PhoneNumber))
        {
            await _identityService.DeactivateUserAsync(employee.PhoneNumber);
        }

        return true;
    }
}
