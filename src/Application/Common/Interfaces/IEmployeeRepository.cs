namespace AttendanceMaSys.Application.Common.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<List<Employee>> GetAllAsync(CancellationToken ct = default);
    Task<List<Employee>> GetByDepartmentAsync(Department department, CancellationToken ct = default);
    Task<Guid> AddAsync(Employee employee, CancellationToken ct = default);
    Task AddBatchAsync(IEnumerable<Employee> employees, CancellationToken ct = default);
    Task UpdateAsync(Employee employee, CancellationToken ct = default);
}
