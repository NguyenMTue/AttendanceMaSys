namespace AttendanceMaSys.Application.Employees.DTOs;

public class BatchImportEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Gender { get; set; } = "Other"; // Male, Female, Other
    public string Department { get; set; } = "IT"; // IT, HR, Finance, Sales
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsIntern { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = "Employee123!";
    public string EmployeeType { get; set; } = "Employee"; // Developer, QA, Manager, Employee
    public int? Band { get; set; }
    public string? TechnicalDirection { get; set; }
    public bool? CodingSkillsFlag { get; set; }
    public string? ManagerType { get; set; } // DepartmentManager or GeneralManager
}
