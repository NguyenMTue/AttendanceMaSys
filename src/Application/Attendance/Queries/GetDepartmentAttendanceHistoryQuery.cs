using MindVaultAI.Application.Attendance.DTOs;
using MindVaultAI.Application.Common.Interfaces;

namespace MindVaultAI.Application.Attendance.Queries;

public record GetDepartmentAttendanceHistoryQuery(
    Department Department,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<List<AttendanceRecordDto>>;

public class GetDepartmentAttendanceHistoryQueryHandler : IRequestHandler<GetDepartmentAttendanceHistoryQuery, List<AttendanceRecordDto>>
{
    private readonly IAttendanceRepository _attendanceRepository;

    public GetDepartmentAttendanceHistoryQueryHandler(IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<List<AttendanceRecordDto>> Handle(GetDepartmentAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var records = await _attendanceRepository.GetDepartmentHistoryAsync(request.Department, request.StartDate, request.EndDate, cancellationToken);

        return records.Select(r => new AttendanceRecordDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee != null ? $"{r.Employee.FirstName} {r.Employee.LastName}" : "Unknown",
            Department = r.Employee?.Department.ToString() ?? request.Department.ToString(),
            Date = r.Date,
            ArrivalTime = r.ArrivalTime,
            DepartureTime = r.DepartureTime
        }).ToList();
    }
}
