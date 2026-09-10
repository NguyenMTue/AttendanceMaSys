using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AttendanceMaSys.Application.Attendance.DTOs;
using AttendanceMaSys.Application.Auth.DTOs;
using AttendanceMaSys.Application.Employees.DTOs;
using AttendanceMaSys.Domain.Enums;

namespace AttendanceMaSys.ConsoleClient;

/// <summary>
/// ApiClient đảm nhận nhiệm vụ gửi các yêu cầu HTTP RESTful API đến hệ thống Web API Server.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string BaseUrl { get; }

    public ApiClient(string baseUrl)
    {
        BaseUrl = baseUrl.TrimEnd('/');

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }

    public void SetBearerToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<LoginResponseDto> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", new { Email = email, Password = password }, JsonOptions);
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
        return result ?? throw new Exception("Không nhận được phản hồi hợp lệ từ server.");
    }

    public async Task<AttendanceRecordDto> CheckInAsync(Guid employeeId)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Attendance/check-in", new { EmployeeId = employeeId }, JsonOptions);
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<AttendanceRecordDto>(JsonOptions);
        return result ?? throw new Exception("Không nhận được phản hồi hợp lệ từ server.");
    }

    public async Task<AttendanceRecordDto> CheckOutAsync(Guid employeeId)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Attendance/check-out", new { EmployeeId = employeeId }, JsonOptions);
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<AttendanceRecordDto>(JsonOptions);
        return result ?? throw new Exception("Không nhận được phản hồi hợp lệ từ server.");
    }

    public async Task<List<AttendanceRecordDto>> GetPersonalHistoryAsync(Guid employeeId, DateTime? startDate, DateTime? endDate)
    {
        var query = $"/api/Attendance/history/personal?employeeId={employeeId}";
        if (startDate.HasValue) query += $"&startDate={startDate.Value:yyyy-MM-dd}";
        if (endDate.HasValue) query += $"&endDate={endDate.Value:yyyy-MM-dd}";

        var response = await _httpClient.GetAsync(query);
        await EnsureSuccessAsync(response);

        var list = await response.Content.ReadFromJsonAsync<List<AttendanceRecordDto>>(JsonOptions);
        return list ?? new List<AttendanceRecordDto>();
    }

    public async Task<List<AttendanceRecordDto>> GetDepartmentHistoryAsync(Department department, DateTime? startDate, DateTime? endDate)
    {
        var query = $"/api/Attendance/history/department?department={department}";
        if (startDate.HasValue) query += $"&startDate={startDate.Value:yyyy-MM-dd}";
        if (endDate.HasValue) query += $"&endDate={endDate.Value:yyyy-MM-dd}";

        var response = await _httpClient.GetAsync(query);
        await EnsureSuccessAsync(response);

        var list = await response.Content.ReadFromJsonAsync<List<AttendanceRecordDto>>(JsonOptions);
        return list ?? new List<AttendanceRecordDto>();
    }

    public async Task<List<AttendanceRecordDto>> GetAllHistoryAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = "/api/Attendance/history/all";
        var queryParams = new List<string>();
        if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        if (queryParams.Count > 0) query += "?" + string.Join("&", queryParams);

        var response = await _httpClient.GetAsync(query);
        await EnsureSuccessAsync(response);

        var list = await response.Content.ReadFromJsonAsync<List<AttendanceRecordDto>>(JsonOptions);
        return list ?? new List<AttendanceRecordDto>();
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync(Department? department)
    {
        var query = "/api/Employees";
        if (department.HasValue) query += $"?department={department.Value}";

        var response = await _httpClient.GetAsync(query);
        await EnsureSuccessAsync(response);

        var list = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>(JsonOptions);
        return list ?? new List<EmployeeDto>();
    }

    public async Task<BatchImportResultDto> BatchImportAsync(List<BatchImportEmployeeDto> employees)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Employees/batch-import", new { Employees = employees }, JsonOptions);
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<BatchImportResultDto>(JsonOptions);
        return result ?? throw new Exception("Không nhận được phản hồi hợp lệ từ server.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Phiên đăng nhập hết hạn hoặc tài khoản không có quyền truy cập.");
            }
            throw new Exception($"Lỗi Server ({response.StatusCode}): {content}");
        }
    }
}
