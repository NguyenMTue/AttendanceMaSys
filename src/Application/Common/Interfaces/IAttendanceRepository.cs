namespace MindVaultAI.Application.Common.Interfaces;

public interface IAttendanceRepository
{
    Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AttendanceRecord?> GetTodayRecordAsync(Guid employeeId, CancellationToken ct = default);
    Task<List<AttendanceRecord>> GetHistoryAsync(Guid employeeId, DateTime? startDate, DateTime? endDate, CancellationToken ct = default);
    Task<List<AttendanceRecord>> GetDepartmentHistoryAsync(Department department, DateTime? startDate, DateTime? endDate, CancellationToken ct = default);
    Task<List<AttendanceRecord>> GetAllHistoryAsync(DateTime? startDate, DateTime? endDate, CancellationToken ct = default);
    Task<Guid> AddAsync(AttendanceRecord record, CancellationToken ct = default);
    Task UpdateAsync(AttendanceRecord record, CancellationToken ct = default);
}
