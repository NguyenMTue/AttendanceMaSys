using Microsoft.EntityFrameworkCore;
using MindVaultAI.Domain.Entities;

namespace MindVaultAI.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Employee> Employees { get; }

    DbSet<AttendanceRecord> AttendanceRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
