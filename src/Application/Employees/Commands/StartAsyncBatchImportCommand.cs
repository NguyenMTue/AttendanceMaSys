using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.DTOs;
using AttendanceMaSys.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceMaSys.Application.Employees.Commands;

public record StartAsyncBatchImportCommand(List<BatchImportEmployeeDto> Employees) : IRequest<Guid>;

public class StartAsyncBatchImportCommandHandler : IRequestHandler<StartAsyncBatchImportCommand, Guid>
{
    private readonly IImportJobStore _importJobStore;
    private readonly IServiceProvider _serviceProvider;

    public StartAsyncBatchImportCommandHandler(
        IImportJobStore importJobStore,
        IServiceProvider serviceProvider)
    {
        _importJobStore = importJobStore;
        _serviceProvider = serviceProvider;
    }

    public Task<Guid> Handle(StartAsyncBatchImportCommand request, CancellationToken cancellationToken)
    {
        var jobId = _importJobStore.CreateJob(request.Employees.Count);

        // Fire and forget background process for asynchronous execution
        _ = Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
            var employeeRepository = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();

            _importJobStore.UpdateJobStatus(jobId, "Processing");

            var employeesToAdd = new List<Employee>();

            for (int i = 0; i < request.Employees.Count; i++)
            {
                var item = request.Employees[i];
                var rowIndex = i + 1;

                try
                {
                    Enum.TryParse<Gender>(item.Gender, true, out var gender);
                    Enum.TryParse<Department>(item.Department, true, out var dept);

                    RoleEnum role = RoleEnum.Employee;
                    if ((item.EmployeeType ?? "").Equals("Manager", StringComparison.OrdinalIgnoreCase))
                    {
                        role = item.ManagerType?.Equals("GeneralManager", StringComparison.OrdinalIgnoreCase) == true
                            ? RoleEnum.GeneralManager
                            : RoleEnum.DepartmentManager;
                    }

                    // 1. Create Identity Account
                    var (userResult, userId) = await identityService.CreateUserWithRoleAsync(item.Email, item.Password, role.ToString());
                    if (!userResult.Succeeded)
                    {
                        var errMessage = string.Join("; ", userResult.Errors);
                        _importJobStore.IncrementProgress(jobId, isSuccess: false, rowIndex, item.Email, errMessage);
                        continue;
                    }

                    // 2. Create Domain Entity
                    Employee emp;
                    var empType = item.EmployeeType ?? "Employee";
                    if (empType.Equals("Developer", StringComparison.OrdinalIgnoreCase))
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
                    else if (empType.Equals("QA", StringComparison.OrdinalIgnoreCase))
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
                    else if (empType.Equals("Manager", StringComparison.OrdinalIgnoreCase))
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
                    _importJobStore.IncrementProgress(jobId, isSuccess: true, rowIndex, item.Email);
                }
                catch (Exception ex)
                {
                    _importJobStore.IncrementProgress(jobId, isSuccess: false, rowIndex, item.Email, ex.Message);
                }
            }

            if (employeesToAdd.Count > 0)
            {
                try
                {
                    await employeeRepository.AddBatchAsync(employeesToAdd, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _importJobStore.FailJob(jobId, $"Lỗi khi lưu dữ liệu nhân viên vào CSDL: {ex.Message}");
                    return;
                }
            }

            _importJobStore.CompleteJob(jobId);
        });

        return Task.FromResult(jobId);
    }
}
