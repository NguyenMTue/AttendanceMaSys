namespace AttendanceMaSys.Application.Employees.DTOs;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsIntern { get; set; }
    public string Role { get; set; } = string.Empty;
    public string EmployeeType { get; set; } = string.Empty;
    public int? Band { get; set; }
    public string? TechnicalDirection { get; set; }
    public bool? CodingSkillsFlag { get; set; }
    public string? ManagerType { get; set; }
}
