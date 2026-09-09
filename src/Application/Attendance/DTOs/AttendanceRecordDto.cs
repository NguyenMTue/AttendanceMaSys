namespace MindVaultAI.Application.Attendance.DTOs;

public class AttendanceRecordDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }
}
