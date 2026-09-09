using System.Data;
using AttendanceMaSys.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;

namespace AttendanceMaSys.Infrastructure.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly string _connectionString;

    public AttendanceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = @"SELECT a.Id, a.EmployeeId, a.Date, a.ArrivalTime, a.DepartureTime,
                                    e.FirstName, e.LastName, e.Department
                             FROM AttendanceRecords a
                             INNER JOIN Employees e ON a.EmployeeId = e.Id
                             WHERE a.Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapReaderToAttendanceRecord(reader);
        }

        return null;
    }

    public async Task<AttendanceRecord?> GetTodayRecordAsync(Guid employeeId, CancellationToken ct = default)
    {
        const string sql = @"SELECT a.Id, a.EmployeeId, a.Date, a.ArrivalTime, a.DepartureTime,
                                    e.FirstName, e.LastName, e.Department
                             FROM AttendanceRecords a
                             INNER JOIN Employees e ON a.EmployeeId = e.Id
                             WHERE a.EmployeeId = @EmployeeId AND a.Date = @TodayDate";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@EmployeeId", employeeId);
        command.Parameters.AddWithValue("@TodayDate", DateTime.Now.Date);

        using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapReaderToAttendanceRecord(reader);
        }

        return null;
    }

    public async Task<List<AttendanceRecord>> GetHistoryAsync(Guid employeeId, DateTime? startDate, DateTime? endDate, CancellationToken ct = default)
    {
        var sql = @"SELECT a.Id, a.EmployeeId, a.Date, a.ArrivalTime, a.DepartureTime,
                           e.FirstName, e.LastName, e.Department
                    FROM AttendanceRecords a
                    INNER JOIN Employees e ON a.EmployeeId = e.Id
                    WHERE a.EmployeeId = @EmployeeId";

        if (startDate.HasValue) sql += " AND a.Date >= @StartDate";
        if (endDate.HasValue) sql += " AND a.Date <= @EndDate";
        sql += " ORDER BY a.Date DESC, a.ArrivalTime DESC";

        var result = new List<AttendanceRecord>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@EmployeeId", employeeId);
        if (startDate.HasValue) command.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
        if (endDate.HasValue) command.Parameters.AddWithValue("@EndDate", endDate.Value.Date);

        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(MapReaderToAttendanceRecord(reader));
        }

        return result;
    }

    public async Task<List<AttendanceRecord>> GetDepartmentHistoryAsync(Department department, DateTime? startDate, DateTime? endDate, CancellationToken ct = default)
    {
        var sql = @"SELECT a.Id, a.EmployeeId, a.Date, a.ArrivalTime, a.DepartureTime,
                           e.FirstName, e.LastName, e.Department
                    FROM AttendanceRecords a
                    INNER JOIN Employees e ON a.EmployeeId = e.Id
                    WHERE e.Department = @Department";

        if (startDate.HasValue) sql += " AND a.Date >= @StartDate";
        if (endDate.HasValue) sql += " AND a.Date <= @EndDate";
        sql += " ORDER BY a.Date DESC, a.ArrivalTime DESC";

        var result = new List<AttendanceRecord>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Department", (int)department);
        if (startDate.HasValue) command.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
        if (endDate.HasValue) command.Parameters.AddWithValue("@EndDate", endDate.Value.Date);

        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(MapReaderToAttendanceRecord(reader));
        }

        return result;
    }

    public async Task<List<AttendanceRecord>> GetAllHistoryAsync(DateTime? startDate, DateTime? endDate, CancellationToken ct = default)
    {
        var sql = @"SELECT a.Id, a.EmployeeId, a.Date, a.ArrivalTime, a.DepartureTime,
                           e.FirstName, e.LastName, e.Department
                    FROM AttendanceRecords a
                    INNER JOIN Employees e ON a.EmployeeId = e.Id
                    WHERE 1=1";

        if (startDate.HasValue) sql += " AND a.Date >= @StartDate";
        if (endDate.HasValue) sql += " AND a.Date <= @EndDate";
        sql += " ORDER BY a.Date DESC, a.ArrivalTime DESC";

        var result = new List<AttendanceRecord>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        if (startDate.HasValue) command.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
        if (endDate.HasValue) command.Parameters.AddWithValue("@EndDate", endDate.Value.Date);

        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(MapReaderToAttendanceRecord(reader));
        }

        return result;
    }

    public async Task<Guid> AddAsync(AttendanceRecord record, CancellationToken ct = default)
    {
        const string sql = @"INSERT INTO AttendanceRecords (Id, EmployeeId, Date, ArrivalTime, DepartureTime)
                             VALUES (@Id, @EmployeeId, @Date, @ArrivalTime, @DepartureTime)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", record.Id == Guid.Empty ? Guid.NewGuid() : record.Id);
        command.Parameters.AddWithValue("@EmployeeId", record.EmployeeId);
        command.Parameters.AddWithValue("@Date", record.Date.Date);
        command.Parameters.AddWithValue("@ArrivalTime", record.ArrivalTime);
        command.Parameters.AddWithValue("@DepartureTime", (object?)record.DepartureTime ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(ct);

        return record.Id;
    }

    public async Task UpdateAsync(AttendanceRecord record, CancellationToken ct = default)
    {
        const string sql = @"UPDATE AttendanceRecords 
                             SET DepartureTime = @DepartureTime 
                             WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", record.Id);
        command.Parameters.AddWithValue("@DepartureTime", (object?)record.DepartureTime ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(ct);
    }

    private static AttendanceRecord MapReaderToAttendanceRecord(SqlDataReader reader)
    {
        var record = new AttendanceRecord
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            EmployeeId = reader.GetGuid(reader.GetOrdinal("EmployeeId")),
            Date = reader.GetDateTime(reader.GetOrdinal("Date")),
            ArrivalTime = reader.GetDateTime(reader.GetOrdinal("ArrivalTime")),
            DepartureTime = reader.IsDBNull(reader.GetOrdinal("DepartureTime")) ? null : reader.GetDateTime(reader.GetOrdinal("DepartureTime"))
        };

        if (!reader.IsDBNull(reader.GetOrdinal("FirstName")))
        {
            record.Employee = new Employee
            {
                Id = record.EmployeeId,
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Department = (Department)reader.GetInt32(reader.GetOrdinal("Department"))
            };
        }

        return record;
    }
}
