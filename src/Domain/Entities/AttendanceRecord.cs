namespace MindVaultAI.Domain.Entities;

public class AttendanceRecord : BaseEntity<Guid>
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }

    public Employee? Employee { get; set; }
}
