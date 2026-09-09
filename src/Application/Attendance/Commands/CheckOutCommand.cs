using MindVaultAI.Application.Attendance.DTOs;
using MindVaultAI.Application.Common.Interfaces;

namespace MindVaultAI.Application.Attendance.Commands;

public record CheckOutCommand(Guid EmployeeId) : IRequest<AttendanceRecordDto>;

public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, AttendanceRecordDto>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public CheckOutCommandHandler(
        IAttendanceRepository attendanceRepository,
        IEmployeeRepository employeeRepository)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<AttendanceRecordDto> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            throw new NotFoundException(nameof(Employee), request.EmployeeId.ToString());
        }

        var todayRecord = await _attendanceRepository.GetTodayRecordAsync(request.EmployeeId, cancellationToken);
        if (todayRecord == null)
        {
            throw new InvalidOperationException("Bạn chưa thực hiện check-in hôm nay.");
        }

        if (todayRecord.DepartureTime.HasValue)
        {
            throw new InvalidOperationException("Bạn đã thực hiện check-out hôm nay rồi.");
        }

        todayRecord.DepartureTime = DateTime.Now;
        await _attendanceRepository.UpdateAsync(todayRecord, cancellationToken);

        return new AttendanceRecordDto
        {
            Id = todayRecord.Id,
            EmployeeId = todayRecord.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            Department = employee.Department.ToString(),
            Date = todayRecord.Date,
            ArrivalTime = todayRecord.ArrivalTime,
            DepartureTime = todayRecord.DepartureTime
        };
    }
}
