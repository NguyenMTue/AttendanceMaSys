using AttendanceMaSys.Application.Attendance.DTOs;
using AttendanceMaSys.Application.Common.Interfaces;

namespace AttendanceMaSys.Application.Attendance.Queries;

public record GetPersonalAttendanceHistoryQuery(
    Guid EmployeeId,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<List<AttendanceRecordDto>>;

public class GetPersonalAttendanceHistoryQueryHandler : IRequestHandler<GetPersonalAttendanceHistoryQuery, List<AttendanceRecordDto>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public GetPersonalAttendanceHistoryQueryHandler(
        IAttendanceRepository attendanceRepository,
        IEmployeeRepository employeeRepository)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<List<AttendanceRecordDto>> Handle(GetPersonalAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        var empName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown";
        var deptName = employee?.Department.ToString() ?? "";

        var records = await _attendanceRepository.GetHistoryAsync(request.EmployeeId, request.StartDate, request.EndDate, cancellationToken);

        return records.Select(r => new AttendanceRecordDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee != null ? $"{r.Employee.FirstName} {r.Employee.LastName}" : empName,
            Department = r.Employee?.Department.ToString() ?? deptName,
            Date = r.Date,
            ArrivalTime = r.ArrivalTime,
            DepartureTime = r.DepartureTime
        }).ToList();
    }
}
