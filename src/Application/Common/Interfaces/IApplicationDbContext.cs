using Microsoft.EntityFrameworkCore;
using AttendanceMaSys.Domain.Entities;

namespace AttendanceMaSys.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Employee> Employees { get; }

    DbSet<AttendanceRecord> AttendanceRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
