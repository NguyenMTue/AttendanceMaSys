using AttendanceMaSys.Application.Attendance.DTOs;
using AttendanceMaSys.Application.Common.Interfaces;

namespace AttendanceMaSys.Application.Attendance.Commands;

public record CheckInCommand(Guid EmployeeId) : IRequest<AttendanceRecordDto>;

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, AttendanceRecordDto>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public CheckInCommandHandler(
        IAttendanceRepository attendanceRepository,
        IEmployeeRepository employeeRepository)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<AttendanceRecordDto> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            throw new NotFoundException(nameof(Employee), request.EmployeeId.ToString());
        }

        if (!employee.IsActive)
        {
            throw new InvalidOperationException("Tài khoản nhân viên đã bị vô hiệu hóa / nhân viên đã nghỉ việc.");
        }

        var existingRecord = await _attendanceRepository.GetTodayRecordAsync(request.EmployeeId, cancellationToken);
        if (existingRecord != null)
        {
            throw new InvalidOperationException("Bạn đã thực hiện check-in hôm nay rồi.");
        }

        var now = DateTime.Now;
        var record = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            Date = now.Date,
            ArrivalTime = now,
            DepartureTime = null
        };

        await _attendanceRepository.AddAsync(record, cancellationToken);

        return new AttendanceRecordDto
        {
            Id = record.Id,
            EmployeeId = record.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            Department = employee.Department.ToString(),
            Date = record.Date,
            ArrivalTime = record.ArrivalTime,
            DepartureTime = record.DepartureTime
        };
    }
}
