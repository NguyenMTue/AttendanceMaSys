using System.Reflection;
using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Domain.Entities;
using AttendanceMaSys.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AttendanceMaSys.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<Employee>()
            .HasDiscriminator<string>("EmployeeType")
            .HasValue<Employee>("Employee")
            .HasValue<Developer>("Developer")
            .HasValue<QA>("QA")
            .HasValue<Manager>("Manager");
    }
}
