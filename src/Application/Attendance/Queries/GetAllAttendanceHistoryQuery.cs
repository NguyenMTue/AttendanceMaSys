using AttendanceMaSys.Application.Attendance.DTOs;
using AttendanceMaSys.Application.Common.Interfaces;

namespace AttendanceMaSys.Application.Attendance.Queries;

public record GetAllAttendanceHistoryQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<List<AttendanceRecordDto>>;

public class GetAllAttendanceHistoryQueryHandler : IRequestHandler<GetAllAttendanceHistoryQuery, List<AttendanceRecordDto>>
{
    private readonly IAttendanceRepository _attendanceRepository;

    public GetAllAttendanceHistoryQueryHandler(IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<List<AttendanceRecordDto>> Handle(GetAllAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var records = await _attendanceRepository.GetAllHistoryAsync(request.StartDate, request.EndDate, cancellationToken);

        return records.Select(r => new AttendanceRecordDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee != null ? $"{r.Employee.FirstName} {r.Employee.LastName}" : "Unknown",
            Department = r.Employee?.Department.ToString() ?? "",
            Date = r.Date,
            ArrivalTime = r.ArrivalTime,
            DepartureTime = r.DepartureTime
        }).ToList();
    }
}
