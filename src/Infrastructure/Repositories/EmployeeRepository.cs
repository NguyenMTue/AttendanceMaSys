using System.Data;
using AttendanceMaSys.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;

namespace AttendanceMaSys.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly string _connectionString;

    public EmployeeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = @"SELECT Id, FirstName, LastName, Gender, Department, PhoneNumber, IsIntern, Role, EmployeeType, Band, TechnicalDirection, CodingSkillsFlag, ManagerType 
                             FROM Employees WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapReaderToEmployee(reader);
        }

        return null;
    }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        const string sql = @"SELECT e.Id, e.FirstName, e.LastName, e.Gender, e.Department, e.PhoneNumber, e.IsIntern, e.Role, e.EmployeeType, e.Band, e.TechnicalDirection, e.CodingSkillsFlag, e.ManagerType 
                             FROM Employees e
                             INNER JOIN AspNetUsers u ON (u.Email = @Email OR u.UserName = @Email)
                             WHERE e.PhoneNumber = u.PhoneNumber OR e.PhoneNumber = u.Email";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapReaderToEmployee(reader);
        }

        return null;
    }

    public async Task<List<Employee>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = @"SELECT Id, FirstName, LastName, Gender, Department, PhoneNumber, IsIntern, Role, EmployeeType, Band, TechnicalDirection, CodingSkillsFlag, ManagerType 
                             FROM Employees";

        var result = new List<Employee>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(MapReaderToEmployee(reader));
        }

        return result;
    }

    public async Task<List<Employee>> GetByDepartmentAsync(Department department, CancellationToken ct = default)
    {
        const string sql = @"SELECT Id, FirstName, LastName, Gender, Department, PhoneNumber, IsIntern, Role, EmployeeType, Band, TechnicalDirection, CodingSkillsFlag, ManagerType 
                             FROM Employees WHERE Department = @Department";

        var result = new List<Employee>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Department", (int)department);

        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(MapReaderToEmployee(reader));
        }

        return result;
    }

    public async Task<Guid> AddAsync(Employee employee, CancellationToken ct = default)
    {
        await AddBatchAsync([employee], ct);
        return employee.Id;
    }

    public async Task AddBatchAsync(IEnumerable<Employee> employees, CancellationToken ct = default)
    {
        const string sql = @"INSERT INTO Employees (Id, FirstName, LastName, Gender, Department, PhoneNumber, IsIntern, Role, EmployeeType, Band, TechnicalDirection, CodingSkillsFlag, ManagerType)
                             VALUES (@Id, @FirstName, @LastName, @Gender, @Department, @PhoneNumber, @IsIntern, @Role, @EmployeeType, @Band, @TechnicalDirection, @CodingSkillsFlag, @ManagerType)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var emp in employees)
            {
                using var command = new SqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@Id", emp.Id == Guid.Empty ? Guid.NewGuid() : emp.Id);
                command.Parameters.AddWithValue("@FirstName", emp.FirstName);
                command.Parameters.AddWithValue("@LastName", emp.LastName);
                command.Parameters.AddWithValue("@Gender", (int)emp.Gender);
                command.Parameters.AddWithValue("@Department", (int)emp.Department);
                command.Parameters.AddWithValue("@PhoneNumber", emp.PhoneNumber ?? string.Empty);
                command.Parameters.AddWithValue("@IsIntern", emp.IsIntern);
                command.Parameters.AddWithValue("@Role", (int)emp.Role);

                if (emp is Developer dev)
                {
                    command.Parameters.AddWithValue("@EmployeeType", "Developer");
                    command.Parameters.AddWithValue("@Band", dev.Band);
                    command.Parameters.AddWithValue("@TechnicalDirection", dev.TechnicalDirection ?? string.Empty);
                    command.Parameters.AddWithValue("@CodingSkillsFlag", DBNull.Value);
                    command.Parameters.AddWithValue("@ManagerType", DBNull.Value);
                }
                else if (emp is QA qa)
                {
                    command.Parameters.AddWithValue("@EmployeeType", "QA");
                    command.Parameters.AddWithValue("@Band", qa.Band);
                    command.Parameters.AddWithValue("@TechnicalDirection", DBNull.Value);
                    command.Parameters.AddWithValue("@CodingSkillsFlag", qa.CodingSkillsFlag);
                    command.Parameters.AddWithValue("@ManagerType", DBNull.Value);
                }
                else if (emp is Manager mgr)
                {
                    command.Parameters.AddWithValue("@EmployeeType", "Manager");
                    command.Parameters.AddWithValue("@Band", DBNull.Value);
                    command.Parameters.AddWithValue("@TechnicalDirection", DBNull.Value);
                    command.Parameters.AddWithValue("@CodingSkillsFlag", DBNull.Value);
                    command.Parameters.AddWithValue("@ManagerType", (int)mgr.ManagerType);
                }
                else
                {
                    command.Parameters.AddWithValue("@EmployeeType", "Employee");
                    command.Parameters.AddWithValue("@Band", DBNull.Value);
                    command.Parameters.AddWithValue("@TechnicalDirection", DBNull.Value);
                    command.Parameters.AddWithValue("@CodingSkillsFlag", DBNull.Value);
                    command.Parameters.AddWithValue("@ManagerType", DBNull.Value);
                }

                await command.ExecuteNonQueryAsync(ct);
            }

            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private static Employee MapReaderToEmployee(SqlDataReader reader)
    {
        var empType = reader.GetString(reader.GetOrdinal("EmployeeType"));
        var id = reader.GetGuid(reader.GetOrdinal("Id"));
        var firstName = reader.GetString(reader.GetOrdinal("FirstName"));
        var lastName = reader.GetString(reader.GetOrdinal("LastName"));
        var gender = (Gender)reader.GetInt32(reader.GetOrdinal("Gender"));
        var dept = (Department)reader.GetInt32(reader.GetOrdinal("Department"));
        var phone = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? "" : reader.GetString(reader.GetOrdinal("PhoneNumber"));
        var isIntern = reader.GetBoolean(reader.GetOrdinal("IsIntern"));
        var role = (RoleEnum)reader.GetInt32(reader.GetOrdinal("Role"));

        Employee emp = empType switch
        {
            "Developer" => new Developer
            {
                Band = reader.IsDBNull(reader.GetOrdinal("Band")) ? 0 : reader.GetInt32(reader.GetOrdinal("Band")),
                TechnicalDirection = reader.IsDBNull(reader.GetOrdinal("TechnicalDirection")) ? "" : reader.GetString(reader.GetOrdinal("TechnicalDirection"))
            },
            "QA" => new QA
            {
                Band = reader.IsDBNull(reader.GetOrdinal("Band")) ? 0 : reader.GetInt32(reader.GetOrdinal("Band")),
                CodingSkillsFlag = !reader.IsDBNull(reader.GetOrdinal("CodingSkillsFlag")) && reader.GetBoolean(reader.GetOrdinal("CodingSkillsFlag"))
            },
            "Manager" => new Manager
            {
                ManagerType = reader.IsDBNull(reader.GetOrdinal("ManagerType")) ? role : (RoleEnum)reader.GetInt32(reader.GetOrdinal("ManagerType"))
            },
            _ => new Employee()
        };

        emp.Id = id;
        emp.FirstName = firstName;
        emp.LastName = lastName;
        emp.Gender = gender;
        emp.Department = dept;
        emp.PhoneNumber = phone;
        emp.IsIntern = isIntern;
        emp.Role = role;

        return emp;
    }
}
