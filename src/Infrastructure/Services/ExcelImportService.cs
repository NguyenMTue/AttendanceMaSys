using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.DTOs;
using MiniExcelLibs;

namespace AttendanceMaSys.Infrastructure.Services;

public class ExcelImportService : IExcelImportService
{
    public List<BatchImportEmployeeDto> ReadEmployeesFromExcel(Stream stream)
    {
        var result = new List<BatchImportEmployeeDto>();

        // Query excel as dynamic dictionary list
        var rows = stream.Query(useHeaderRow: true).Cast<IDictionary<string, object>>().ToList();

        foreach (var row in rows)
        {
            if (row == null || row.Values.All(v => v == null || string.IsNullOrWhiteSpace(v.ToString())))
                continue; // Skip empty rows

            var dto = new BatchImportEmployeeDto
            {
                FirstName = GetString(row, "FirstName", "First Name", "Họ và Tên lót", "Họ", "Ho"),
                LastName = GetString(row, "LastName", "Last Name", "Tên", "Ten"),
                Email = GetString(row, "Email", "Tài khoản", "Tai khoan", "Email Address"),
                Password = GetString(row, "Password", "Mật khẩu", "Mat khau"),
                Gender = GetString(row, "Gender", "Giới tính", "Gioi tinh"),
                Department = GetString(row, "Department", "Phòng ban", "Phong ban"),
                PhoneNumber = GetString(row, "PhoneNumber", "Phone", "Số điện thoại", "So dien thoai", "SĐT", "SDT"),
                IsIntern = GetBool(row, "IsIntern", "Intern", "Thực tập", "Thuc tap"),
                EmployeeType = GetString(row, "EmployeeType", "Loại nhân viên", "Loai nhan vien", "Chức vụ"),
                Band = GetInt(row, "Band", "Cấp độ", "Cap do"),
                TechnicalDirection = GetString(row, "TechnicalDirection", "Hướng kỹ thuật", "Huong ky thuat", "Chuyên môn"),
                CodingSkillsFlag = GetBool(row, "CodingSkillsFlag", "Biết code", "Biet code", "Kỹ năng code"),
                ManagerType = GetString(row, "ManagerType", "Loại quản lý", "Loai quan ly")
            };

            // Fallback default values if empty
            if (string.IsNullOrWhiteSpace(dto.Gender)) dto.Gender = "Male";
            if (string.IsNullOrWhiteSpace(dto.Department)) dto.Department = "IT";
            if (string.IsNullOrWhiteSpace(dto.EmployeeType)) dto.EmployeeType = "Employee";

            result.Add(dto);
        }

        return result;
    }

    public byte[] GenerateExcelTemplate()
    {
        var sampleData = new List<BatchImportEmployeeDto>
        {
            new BatchImportEmployeeDto
            {
                FirstName = "Hòa",
                LastName = "Đặng",
                Email = "hoa.dang@company.com",
                Password = "Employee123!",
                Gender = "Male",
                Department = "IT",
                PhoneNumber = "0901234567",
                IsIntern = false,
                EmployeeType = "Developer",
                Band = 2,
                TechnicalDirection = "C# / ASP.NET Core"
            },
            new BatchImportEmployeeDto
            {
                FirstName = "Yến",
                LastName = "Trịnh",
                Email = "yen.trinh@company.com",
                Password = "Employee123!",
                Gender = "Female",
                Department = "IT",
                PhoneNumber = "0902345678",
                IsIntern = false,
                EmployeeType = "QA",
                Band = 3,
                CodingSkillsFlag = true
            },
            new BatchImportEmployeeDto
            {
                FirstName = "Phong",
                LastName = "Lê",
                Email = "phong.le@company.com",
                Password = "Manager123!",
                Gender = "Male",
                Department = "HR",
                PhoneNumber = "0903456789",
                IsIntern = false,
                EmployeeType = "Manager",
                ManagerType = "DepartmentManager"
            }
        };

        using var memoryStream = new MemoryStream();
        memoryStream.SaveAs(sampleData);
        return memoryStream.ToArray();
    }

    private static string GetString(IDictionary<string, object> row, params string[] keys)
    {
        // 1. Try exact match (case insensitive)
        foreach (var key in keys)
        {
            var match = row.FirstOrDefault(k => k.Key != null && string.Equals(k.Key.Trim(), key.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match.Key != null && match.Value != null)
            {
                var val = match.Value.ToString()?.Trim();
                if (!string.IsNullOrEmpty(val))
                    return val;
            }
        }

        // 2. Try partial match (candidate key contained in column header or vice versa)
        foreach (var key in keys)
        {
            var match = row.FirstOrDefault(k => k.Key != null && (
                k.Key.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                key.IndexOf(k.Key.Trim(), StringComparison.OrdinalIgnoreCase) >= 0
            ));
            if (match.Key != null && match.Value != null)
            {
                var val = match.Value.ToString()?.Trim();
                if (!string.IsNullOrEmpty(val))
                    return val;
            }
        }

        return string.Empty;
    }

    private static bool GetBool(IDictionary<string, object> row, params string[] keys)
    {
        var str = GetString(row, keys);
        if (string.IsNullOrWhiteSpace(str)) return false;
        if (bool.TryParse(str, out var b)) return b;
        return str.Equals("có", StringComparison.OrdinalIgnoreCase) ||
               str.Equals("true", StringComparison.OrdinalIgnoreCase) ||
               str.Equals("1", StringComparison.OrdinalIgnoreCase) ||
               str.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    private static int? GetInt(IDictionary<string, object> row, params string[] keys)
    {
        var str = GetString(row, keys);
        if (int.TryParse(str, out var val)) return val;
        return null;
    }
}
