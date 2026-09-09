using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.DTOs;

namespace AttendanceMaSys.Application.Employees.Commands;

public record BatchImportEmployeesCommand(List<BatchImportEmployeeDto> Employees) : IRequest<BatchImportResultDto>;

public class BatchImportEmployeesCommandHandler : IRequestHandler<BatchImportEmployeesCommand, BatchImportResultDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IIdentityService _identityService;

    public BatchImportEmployeesCommandHandler(
        IEmployeeRepository employeeRepository,
        IIdentityService identityService)
    {
        _employeeRepository = employeeRepository;
        _identityService = identityService;
    }

    public async Task<BatchImportResultDto> Handle(BatchImportEmployeesCommand request, CancellationToken cancellationToken)
    {
        var result = new BatchImportResultDto();
        var employeesToAdd = new List<Employee>();

        foreach (var item in request.Employees)
        {
            try
            {
                Enum.TryParse<Gender>(item.Gender, true, out var gender);
                Enum.TryParse<Department>(item.Department, true, out var dept);

                RoleEnum role = RoleEnum.Employee;
                if (item.EmployeeType.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                {
                    role = item.ManagerType?.Equals("GeneralManager", StringComparison.OrdinalIgnoreCase) == true
                        ? RoleEnum.GeneralManager
                        : RoleEnum.DepartmentManager;
                }

                // 1. Create Identity Account
                var (userResult, userId) = await _identityService.CreateUserWithRoleAsync(item.Email, item.Password, role.ToString());
                if (!userResult.Succeeded)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Tài khoản {item.Email} thất bại: {string.Join(", ", userResult.Errors)}");
                    continue;
                }

                // 2. Create Domain Entity
                Employee emp;
                if (item.EmployeeType.Equals("Developer", StringComparison.OrdinalIgnoreCase))
                {
                    emp = new Developer
                    {
                        Id = Guid.NewGuid(),
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Gender = gender,
                        Department = dept,
                        PhoneNumber = item.PhoneNumber,
                        IsIntern = item.IsIntern,
                        Role = role,
                        Band = item.Band ?? 1,
                        TechnicalDirection = item.TechnicalDirection ?? "General"
                    };
                }
                else if (item.EmployeeType.Equals("QA", StringComparison.OrdinalIgnoreCase))
                {
                    emp = new QA
                    {
                        Id = Guid.NewGuid(),
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Gender = gender,
                        Department = dept,
                        PhoneNumber = item.PhoneNumber,
                        IsIntern = item.IsIntern,
                        Role = role,
                        Band = item.Band ?? 1,
                        CodingSkillsFlag = item.CodingSkillsFlag ?? false
                    };
                }
                else if (item.EmployeeType.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                {
                    emp = new Manager
                    {
                        Id = Guid.NewGuid(),
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Gender = gender,
                        Department = dept,
                        PhoneNumber = item.PhoneNumber,
                        IsIntern = item.IsIntern,
                        Role = role,
                        ManagerType = role
                    };
                }
                else
                {
                    emp = new Employee
                    {
                        Id = Guid.NewGuid(),
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Gender = gender,
                        Department = dept,
                        PhoneNumber = item.PhoneNumber,
                        IsIntern = item.IsIntern,
                        Role = role
                    };
                }

                employeesToAdd.Add(emp);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add($"Lỗi tạo nhân viên {item.FirstName} {item.LastName}: {ex.Message}");
            }
        }

        if (employeesToAdd.Count > 0)
        {
            await _employeeRepository.AddBatchAsync(employeesToAdd, cancellationToken);
        }

        return result;
    }
}
