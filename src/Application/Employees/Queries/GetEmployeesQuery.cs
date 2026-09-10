using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.DTOs;

namespace AttendanceMaSys.Application.Employees.Queries;

public record GetEmployeesQuery(Department? Department = null) : IRequest<List<EmployeeDto>>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, List<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<List<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        List<Employee> employees;

        if (request.Department.HasValue)
        {
            employees = await _employeeRepository.GetByDepartmentAsync(request.Department.Value, cancellationToken);
        }
        else
        {
            employees = await _employeeRepository.GetAllAsync(cancellationToken);
        }

        return employees.Select(MapToDto).ToList();
    }

    private static EmployeeDto MapToDto(Employee emp)
    {
        var dto = new EmployeeDto
        {
            Id = emp.Id,
            FirstName = emp.FirstName,
            LastName = emp.LastName,
            Gender = emp.Gender.ToString(),
            Department = emp.Department.ToString(),
            PhoneNumber = emp.PhoneNumber,
            IsIntern = emp.IsIntern,
            Role = emp.Role.ToString(),
            EmployeeType = emp.GetType().Name,
            IsActive = emp.IsActive
        };

        if (emp is Developer dev)
        {
            dto.Band = dev.Band;
            dto.TechnicalDirection = dev.TechnicalDirection;
        }
        else if (emp is QA qa)
        {
            dto.Band = qa.Band;
            dto.CodingSkillsFlag = qa.CodingSkillsFlag;
        }
        else if (emp is Manager mgr)
        {
            dto.ManagerType = mgr.ManagerType.ToString();
        }

        return dto;
    }
}
