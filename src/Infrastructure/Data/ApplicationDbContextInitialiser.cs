using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Domain.Constants;
using AttendanceMaSys.Domain.Entities;
using AttendanceMaSys.Domain.Enums;
using AttendanceMaSys.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AttendanceMaSys.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendanceRepository _attendanceRepository;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmployeeRepository employeeRepository,
        IAttendanceRepository attendanceRepository)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _employeeRepository = employeeRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();

            if (_context.Database.IsSqlServer())
            {
                const string ensureIsActiveColumnSql = @"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Employees')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Employees' AND COLUMN_NAME = 'IsActive')
                        BEGIN
                            ALTER TABLE Employees ADD IsActive BIT NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT 1;
                        END
                    END";

                await _context.Database.ExecuteSqlRawAsync(ensureIsActiveColumnSql);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // 1. Seed Roles
        string[] roles = [Roles.Admin, Roles.GeneralManager, Roles.DepartmentManager, Roles.Employee];
        foreach (var roleName in roles)
        {
            if (_roleManager.Roles.All(r => r.Name != roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Define user accounts & corresponding Domain entities
        var userSeeds = new (string Email, string Password, string Role, string Phone, Employee Employee)[]
        {
            // Admin
            ("admin@company.com", "Admin123!", Roles.Admin, "0900000001", new Manager
            {
                FirstName = "System",
                LastName = "Admin",
                Gender = Gender.Male,
                Department = Department.IT,
                PhoneNumber = "0900000001",
                Role = RoleEnum.Admin,
                ManagerType = RoleEnum.Admin,
                IsIntern = false
            }),

            // General Manager (Giám Đốc)
            ("gm@company.com", "Manager123!", Roles.GeneralManager, "0900000002", new Manager
            {
                FirstName = "Văn",
                LastName = "Giám Đốc",
                Gender = Gender.Male,
                Department = Department.IT,
                PhoneNumber = "0900000002",
                Role = RoleEnum.GeneralManager,
                ManagerType = RoleEnum.GeneralManager,
                IsIntern = false
            }),

            // Department Manager (Trưởng Phòng IT)
            ("deptmanager.it@company.com", "Manager123!", Roles.DepartmentManager, "0900000003", new Manager
            {
                FirstName = "Minh",
                LastName = "Trưởng Phòng IT",
                Gender = Gender.Male,
                Department = Department.IT,
                PhoneNumber = "0900000003",
                Role = RoleEnum.DepartmentManager,
                ManagerType = RoleEnum.DepartmentManager,
                IsIntern = false
            }),

            // Department Manager (Trưởng Phòng HR)
            ("deptmanager.hr@company.com", "Manager123!", Roles.DepartmentManager, "0900000004", new Manager
            {
                FirstName = "Thu",
                LastName = "Trưởng Phòng HR",
                Gender = Gender.Female,
                Department = Department.HR,
                PhoneNumber = "0900000004",
                Role = RoleEnum.DepartmentManager,
                ManagerType = RoleEnum.DepartmentManager,
                IsIntern = false
            }),

            // --- Full IT Team ---
            // 1. Tech Lead Developer
            ("lead.dev@company.com", "Employee123!", Roles.Employee, "0901000001", new Developer
            {
                FirstName = "Tuấn",
                LastName = "Trần Lead",
                Gender = Gender.Male,
                Department = Department.IT,
                PhoneNumber = "0901000001",
                Role = RoleEnum.Employee,
                Band = 4,
                TechnicalDirection = "Backend (.NET / Architecture)",
                IsIntern = false
            }),

            // 2. Fullstack Developer
            ("fullstack.dev@company.com", "Employee123!", Roles.Employee, "0901000002", new Developer
            {
                FirstName = "Hùng",
                LastName = "Nguyễn Dev",
                Gender = Gender.Male,
                Department = Department.IT,
                PhoneNumber = "0901000002",
                Role = RoleEnum.Employee,
                Band = 3,
                TechnicalDirection = "Fullstack (C# & React)",
                IsIntern = false
            }),

            // 3. Frontend Developer
            ("frontend.dev@company.com", "Employee123!", Roles.Employee, "0901000003", new Developer
            {
                FirstName = "Linh",
                LastName = "Phạm Frontend",
                Gender = Gender.Female,
                Department = Department.IT,
                PhoneNumber = "0901000003",
                Role = RoleEnum.Employee,
                Band = 2,
                TechnicalDirection = "Frontend (Vue.js / CSS)",
                IsIntern = false
            }),

            // 4. Junior / Intern Developer
            ("intern.dev@company.com", "Employee123!", Roles.Employee, "0901000004", new Developer
            {
                FirstName = "Nam",
                LastName = "Đỗ Intern",
                Gender = Gender.Male,
                Department = Department.IT,
                PhoneNumber = "0901000004",
                Role = RoleEnum.Employee,
                Band = 1,
                TechnicalDirection = "Backend (C# Web API)",
                IsIntern = true
            }),

            // 5. Senior Automation QA
            ("automation.qa@company.com", "Employee123!", Roles.Employee, "0901000005", new QA
            {
                FirstName = "Hoa",
                LastName = "Lê AutoQA",
                Gender = Gender.Female,
                Department = Department.IT,
                PhoneNumber = "0901000005",
                Role = RoleEnum.Employee,
                Band = 3,
                CodingSkillsFlag = true,
                IsIntern = false
            }),

            // 6. Manual QA
            ("manual.qa@company.com", "Employee123!", Roles.Employee, "0901000006", new QA
            {
                FirstName = "Mai",
                LastName = "Vũ ManualQA",
                Gender = Gender.Female,
                Department = Department.IT,
                PhoneNumber = "0901000006",
                Role = RoleEnum.Employee,
                Band = 2,
                CodingSkillsFlag = false,
                IsIntern = false
            }),

            // --- HR Team ---
            ("employee.hr@company.com", "Employee123!", Roles.Employee, "0902000001", new Employee
            {
                FirstName = "Trang",
                LastName = "Đào HR",
                Gender = Gender.Female,
                Department = Department.HR,
                PhoneNumber = "0902000001",
                Role = RoleEnum.Employee,
                IsIntern = false
            })
        };

        var seededEmployees = new List<Employee>();

        foreach (var (email, password, role, phone, empEntity) in userSeeds)
        {
            if (_userManager.Users.All(u => u.UserName != email))
            {
                var user = new ApplicationUser { UserName = email, Email = email, PhoneNumber = phone };
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);

                    var existingEmp = await _employeeRepository.GetByEmailAsync(email);
                    if (existingEmp == null)
                    {
                        empEntity.Id = Guid.NewGuid();
                        await _employeeRepository.AddAsync(empEntity);
                        seededEmployees.Add(empEntity);
                    }
                    else
                    {
                        seededEmployees.Add(existingEmp);
                    }
                }
            }
            else
            {
                var existingEmp = await _employeeRepository.GetByEmailAsync(email);
                if (existingEmp != null)
                {
                    seededEmployees.Add(existingEmp);
                }
            }
        }

        // 3. Seed sample Attendance Records if empty
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);

        foreach (var emp in seededEmployees)
        {
            var existingRecord = await _attendanceRepository.GetTodayRecordAsync(emp.Id);
            if (existingRecord == null)
            {
                // Seed yesterday record
                await _attendanceRepository.AddAsync(new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = emp.Id,
                    Date = yesterday,
                    ArrivalTime = yesterday.AddHours(8).AddMinutes(30),
                    DepartureTime = yesterday.AddHours(17).AddMinutes(30)
                });

                // Seed today record (checked in)
                await _attendanceRepository.AddAsync(new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = emp.Id,
                    Date = today,
                    ArrivalTime = today.AddHours(8).AddMinutes(25),
                    DepartureTime = null
                });
            }
        }
    }
}
