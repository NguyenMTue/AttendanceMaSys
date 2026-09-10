using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.DTOs;
using AttendanceMaSys.Domain.Entities;
using AttendanceMaSys.Domain.Enums;

namespace AttendanceMaSys.Application.Employees.Commands;

public record UpdateEmployeePositionCommand(
    Guid EmployeeId,
    Department? Department,
    RoleEnum? Role,
    string? EmployeeType,
    int? Band,
    string? TechnicalDirection,
    bool? CodingSkillsFlag,
    RoleEnum? ManagerType
) : IRequest<EmployeeDto>;

public class UpdateEmployeePositionCommandHandler : IRequestHandler<UpdateEmployeePositionCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IIdentityService _identityService;

    public UpdateEmployeePositionCommandHandler(
        IEmployeeRepository employeeRepository,
        IIdentityService identityService)
    {
        _employeeRepository = employeeRepository;
        _identityService = identityService;
    }

    public async Task<EmployeeDto> Handle(UpdateEmployeePositionCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            throw new NotFoundException(nameof(Employee), request.EmployeeId.ToString());
        }

        if (!employee.IsActive)
        {
            throw new InvalidOperationException("Không thể cập nhật vị trí cho nhân viên đã nghỉ việc / bị sa thải.");
        }

        // 1. Update Department
        if (request.Department.HasValue)
        {
            employee.Department = request.Department.Value;
        }

        // 2. Update Role & Identity Role if promoted/changed
        if (request.Role.HasValue && request.Role.Value != employee.Role)
        {
            employee.Role = request.Role.Value;
            if (!string.IsNullOrEmpty(employee.PhoneNumber))
            {
                await _identityService.UpdateUserRoleAsync(employee.PhoneNumber, request.Role.Value.ToString());
            }
        }

        // 3. Update Specific Sub-Type properties
        if (employee is Developer dev)
        {
            if (request.Band.HasValue) dev.Band = request.Band.Value;
            if (!string.IsNullOrWhiteSpace(request.TechnicalDirection)) dev.TechnicalDirection = request.TechnicalDirection;
        }
        else if (employee is QA qa)
        {
            if (request.Band.HasValue) qa.Band = request.Band.Value;
            if (request.CodingSkillsFlag.HasValue) qa.CodingSkillsFlag = request.CodingSkillsFlag.Value;
        }
        else if (employee is Manager mgr)
        {
            if (request.ManagerType.HasValue) mgr.ManagerType = request.ManagerType.Value;
        }

        await _employeeRepository.UpdateAsync(employee, cancellationToken);

        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Gender = employee.Gender.ToString(),
            Department = employee.Department.ToString(),
            PhoneNumber = employee.PhoneNumber,
            IsIntern = employee.IsIntern,
            Role = employee.Role.ToString(),
            EmployeeType = employee.GetType().Name,
            IsActive = employee.IsActive,
            Band = (employee is Developer d) ? d.Band : (employee is QA q ? q.Band : null),
            TechnicalDirection = (employee is Developer d2) ? d2.TechnicalDirection : null,
            CodingSkillsFlag = (employee is QA q2) ? q2.CodingSkillsFlag : null,
            ManagerType = (employee is Manager m) ? m.ManagerType.ToString() : null
        };
    }
}
