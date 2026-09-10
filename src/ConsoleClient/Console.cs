using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AttendanceMaSys.Application.Attendance.DTOs;
using AttendanceMaSys.Application.Auth.DTOs;
using AttendanceMaSys.Application.Employees.DTOs;
using AttendanceMaSys.Domain.Constants;
using AttendanceMaSys.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AttendanceMaSys.ConsoleClient;

/// <summary>
/// Lớp Console / ConsoleApp đại diện cho giao diện điều khiển (CLI)
/// và tương tác với Web API Server qua HTTP RESTful API.
/// </summary>
public class ConsoleApp
{
    private readonly ApiClient _api;
    private readonly ILogger<ConsoleApp> _logger;

    public LoginResponseDto? CurrentUser { get; private set; }

    public ConsoleApp(IConfiguration configuration, ILogger<ConsoleApp> logger)
    {
        _logger = logger;
        var serverUrl = configuration["ServerUrl"] ?? "http://localhost:5160";
        _api = new ApiClient(serverUrl);
    }

    public async Task RunAsync()
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        System.Console.InputEncoding = Encoding.UTF8;

        bool running = true;
        while (running)
        {
            System.Console.Clear();
            DisplayHeader();

            if (CurrentUser == null)
            {
                running = await ShowGuestMenuAsync();
            }
            else
            {
                running = await ShowUserMenuAsync();
            }
        }

        PrintHeader("CẢM ƠN BẠN ĐÃ SỬ DỤNG HỆ THỐNG ATTENDANCE MANAGEMENT SYSTEM!");
    }

    private void DisplayHeader()
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine("==========================================================================");
        System.Console.WriteLine("        ATTENDANCE MANAGEMENT SYSTEM (HỆ THỐNG QUẢN LÝ ĐIỂM DANH)         ");
        System.Console.WriteLine("==========================================================================");
        System.Console.ResetColor();

        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine($" [SERVER WEB API] {_api.BaseUrl}");
        System.Console.ResetColor();

        if (CurrentUser != null)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine($" [ĐÃ ĐĂNG NHẬP] Người dùng: {CurrentUser.Email} | Vai trò: {CurrentUser.Role} | Phòng ban: {CurrentUser.Department}");
            System.Console.WriteLine("--------------------------------------------------------------------------");
            System.Console.ResetColor();
        }
        else
        {
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.WriteLine(" [TRẠNG THÁI] Chưa đăng nhập");
            System.Console.WriteLine("--------------------------------------------------------------------------");
            System.Console.ResetColor();
        }
    }

    private async Task<bool> ShowGuestMenuAsync()
    {
        System.Console.WriteLine("MENU CHÍNH (DÀNH CHO KHÁCH):");
        System.Console.WriteLine(" 1. Đăng nhập hệ thống qua Server Web API");
        System.Console.WriteLine(" 2. Xem danh sách tài khoản mẫu (Demo Accounts)");
        System.Console.WriteLine(" 0. Thoát ứng dụng");
        System.Console.WriteLine();
        System.Console.Write("Vui lòng chọn chức năng (0-2): ");

        var input = System.Console.ReadLine()?.Trim();
        switch (input)
        {
            case "1":
                await HandleLoginAsync();
                break;
            case "2":
                DisplayDemoAccounts();
                break;
            case "0":
                return false;
            default:
                PrintError("Lựa chọn không hợp lệ. Vui lòng bấm phím bất kỳ để thử lại...");
                System.Console.ReadKey();
                break;
        }
        return true;
    }

    private async Task<bool> ShowUserMenuAsync()
    {
        System.Console.WriteLine("DANH MỤC CHỨC NĂNG (GỬI HTTP API ĐẾN SERVER):");
        System.Console.WriteLine(" 1. Điểm danh vào ca (Check-in)");
        System.Console.WriteLine(" 2. Điểm danh ra ca (Check-out)");
        System.Console.WriteLine(" 3. Xem lịch sử điểm danh cá nhân");

        var role = CurrentUser?.Role ?? "";
        bool isManagerOrAdmin = role.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase) ||
                               role.Equals(Roles.GeneralManager, StringComparison.OrdinalIgnoreCase) ||
                               role.Equals(Roles.DepartmentManager, StringComparison.OrdinalIgnoreCase);

        bool isAdminOrGM = role.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase) ||
                           role.Equals(Roles.GeneralManager, StringComparison.OrdinalIgnoreCase);

        bool isAdmin = role.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase);

        if (isManagerOrAdmin)
        {
            System.Console.WriteLine(" 4. Xem lịch sử điểm danh phòng ban");
            System.Console.WriteLine(" 6. Quản lý / Xem danh sách nhân viên");
        }

        if (isAdminOrGM)
        {
            System.Console.WriteLine(" 5. Xem lịch sử điểm danh toàn công ty");
        }

        if (isAdmin)
        {
            System.Console.WriteLine(" 7. Import nhân viên hàng loạt (Batch Import)");
        }

        System.Console.WriteLine(" 8. Đăng xuất (Logout)");
        System.Console.WriteLine(" 0. Thoát ứng dụng");
        System.Console.WriteLine();
        System.Console.Write("Vui lòng chọn chức năng: ");

        var input = System.Console.ReadLine()?.Trim();
        switch (input)
        {
            case "1":
                await HandleCheckInAsync();
                break;
            case "2":
                await HandleCheckOutAsync();
                break;
            case "3":
                await HandlePersonalHistoryAsync();
                break;
            case "4":
                if (isManagerOrAdmin) await HandleDepartmentHistoryAsync();
                else PrintError("Bạn không có quyền truy cập chức năng này.");
                break;
            case "5":
                if (isAdminOrGM) await HandleAllHistoryAsync();
                else PrintError("Bạn không có quyền truy cập chức năng này.");
                break;
            case "6":
                if (isManagerOrAdmin) await HandleGetEmployeesAsync();
                else PrintError("Bạn không có quyền truy cập chức năng này.");
                break;
            case "7":
                if (isAdmin) await HandleBatchImportAsync();
                else PrintError("Bạn không có quyền truy cập chức năng này.");
                break;
            case "8":
                CurrentUser = null;
                _api.SetBearerToken(null);
                PrintSuccess("Đã đăng xuất thành công.");
                PressAnyKeyToContinue();
                break;
            case "0":
                return false;
            default:
                PrintError("Lựa chọn không hợp lệ. Bấm phím bất kỳ để thử lại...");
                PressAnyKeyToContinue();
                break;
        }
        return true;
    }

    #region Handlers

    public async Task HandleLoginAsync()
    {
        PrintHeader("ĐĂNG NHẬP QUA SERVER WEB API");

        System.Console.Write("Email (Tài khoản): ");
        var email = System.Console.ReadLine()?.Trim() ?? "";

        System.Console.Write("Mật khẩu: ");
        var password = ReadPassword();
        System.Console.WriteLine();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            PrintError("Email và mật khẩu không được để trống!");
            PressAnyKeyToContinue();
            return;
        }

        try
        {
            PrintInfo($"Đang gửi request POST {_api.BaseUrl}/api/Auth/login ...");
            var result = await _api.LoginAsync(email, password);

            CurrentUser = result;
            _api.SetBearerToken(result.Token);

            PrintSuccess($"Đăng nhập thành công! Token JWT đã được áp dụng.");
            System.Console.WriteLine($" - Email: {result.Email}");
            System.Console.WriteLine($" - Vai trò: {result.Role}");
            System.Console.WriteLine($" - Phòng ban: {result.Department}");
            System.Console.WriteLine($" - EmployeeId: {result.EmployeeId}");
        }
        catch (Exception ex)
        {
            PrintError($"Đăng nhập thất bại: {ex.Message}");
            PrintWarning("Lưu ý: Hãy chắc chắn Server Web API (`src/Web`) đang chạy tại URL: " + _api.BaseUrl);
        }

        PressAnyKeyToContinue();
    }

    public async Task HandleCheckInAsync()
    {
        PrintHeader("ĐIỂM DANH VÀO CA (CHECK-IN)");

        if (CurrentUser?.EmployeeId == null)
        {
            PrintError("Tài khoản của bạn chưa được liên kết với hồ sơ nhân viên.");
            PressAnyKeyToContinue();
            return;
        }

        try
        {
            PrintInfo($"Đang gửi request POST {_api.BaseUrl}/api/Attendance/check-in ...");
            var result = await _api.CheckInAsync(CurrentUser.EmployeeId.Value);

            PrintSuccess($"Check-in thành công qua API!");
            System.Console.WriteLine($" - Nhân viên: {result.EmployeeName}");
            System.Console.WriteLine($" - Phòng ban: {result.Department}");
            System.Console.WriteLine($" - Ngày điểm danh: {result.Date:dd/MM/yyyy}");
            System.Console.WriteLine($" - Thời gian vào: {result.ArrivalTime:HH:mm:ss dd/MM/yyyy}");
        }
        catch (Exception ex)
        {
            PrintError($"Check-in thất bại: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    public async Task HandleCheckOutAsync()
    {
        PrintHeader("ĐIỂM DANH RA CA (CHECK-OUT)");

        if (CurrentUser?.EmployeeId == null)
        {
            PrintError("Tài khoản của bạn chưa được liên kết với hồ sơ nhân viên.");
            PressAnyKeyToContinue();
            return;
        }

        try
        {
            PrintInfo($"Đang gửi request POST {_api.BaseUrl}/api/Attendance/check-out ...");
            var result = await _api.CheckOutAsync(CurrentUser.EmployeeId.Value);

            PrintSuccess($"Check-out thành công qua API!");
            System.Console.WriteLine($" - Nhân viên: {result.EmployeeName}");
            System.Console.WriteLine($" - Phòng ban: {result.Department}");
            System.Console.WriteLine($" - Thời gian vào: {result.ArrivalTime:HH:mm:ss}");
            System.Console.WriteLine($" - Thời gian ra: {result.DepartureTime:HH:mm:ss dd/MM/yyyy}");

            if (result.DepartureTime.HasValue)
            {
                var duration = result.DepartureTime.Value - result.ArrivalTime;
                System.Console.WriteLine($" - Tổng thời gian làm việc: {duration.Hours} giờ {duration.Minutes} phút");
            }
        }
        catch (Exception ex)
        {
            PrintError($"Check-out thất bại: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    public async Task HandlePersonalHistoryAsync()
    {
        PrintHeader("LỊCH SỬ ĐIỂM DANH CÁ NHÂN");

        if (CurrentUser?.EmployeeId == null)
        {
            PrintError("Tài khoản của bạn chưa được liên kết với hồ sơ nhân viên.");
            PressAnyKeyToContinue();
            return;
        }

        var (startDate, endDate) = ReadOptionalDateRange();

        try
        {
            PrintInfo($"Đang gửi request GET {_api.BaseUrl}/api/Attendance/history/personal ...");
            var records = await _api.GetPersonalHistoryAsync(CurrentUser.EmployeeId.Value, startDate, endDate);

            RenderAttendanceRecordsTable(records);
        }
        catch (Exception ex)
        {
            PrintError($"Lỗi khi lấy lịch sử điểm danh: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    public async Task HandleDepartmentHistoryAsync()
    {
        PrintHeader("LỊCH SỬ ĐIỂM DANH PHÒNG BAN");

        var department = SelectDepartment(CurrentUser?.Department);
        if (department == null) return;

        var (startDate, endDate) = ReadOptionalDateRange();

        try
        {
            PrintInfo($"Đang gửi request GET {_api.BaseUrl}/api/Attendance/history/department ...");
            var records = await _api.GetDepartmentHistoryAsync(department.Value, startDate, endDate);

            RenderAttendanceRecordsTable(records);
        }
        catch (Exception ex)
        {
            PrintError($"Lỗi khi lấy lịch sử phòng ban: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    public async Task HandleAllHistoryAsync()
    {
        PrintHeader("LỊCH SỬ ĐIỂM DANH TOÀN CÔNG TY");

        var (startDate, endDate) = ReadOptionalDateRange();

        try
        {
            PrintInfo($"Đang gửi request GET {_api.BaseUrl}/api/Attendance/history/all ...");
            var records = await _api.GetAllHistoryAsync(startDate, endDate);

            RenderAttendanceRecordsTable(records);
        }
        catch (Exception ex)
        {
            PrintError($"Lỗi khi lấy lịch sử toàn công ty: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    public async Task HandleGetEmployeesAsync()
    {
        PrintHeader("DANH SÁCH NHÂN VIÊN");

        System.Console.WriteLine("Lọc theo phòng ban:");
        System.Console.WriteLine(" 0. Tất cả phòng ban");
        System.Console.WriteLine(" 1. IT");
        System.Console.WriteLine(" 2. HR");
        System.Console.WriteLine(" 3. Finance");
        System.Console.WriteLine(" 4. Sales");
        System.Console.Write("Chọn option (0-4, Mặc định: 0): ");

        var opt = System.Console.ReadLine()?.Trim();
        Department? departmentFilter = opt switch
        {
            "1" => Department.IT,
            "2" => Department.HR,
            "3" => Department.Finance,
            "4" => Department.Sales,
            _ => null
        };

        try
        {
            PrintInfo($"Đang gửi request GET {_api.BaseUrl}/api/Employees ...");
            var employees = await _api.GetEmployeesAsync(departmentFilter);

            RenderEmployeesTable(employees);
        }
        catch (Exception ex)
        {
            PrintError($"Lỗi khi lấy danh sách nhân viên: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    public async Task HandleBatchImportAsync()
    {
        PrintHeader("IMPORT NHÂN VIÊN HÀNG LOẠT (ADMIN VIA API)");

        System.Console.WriteLine("Chức năng gửi request POST /api/Employees/batch-import.");
        System.Console.WriteLine(" 1. Dùng mẫu dữ liệu test có sẵn (3 nhân viên mới)");
        System.Console.WriteLine(" 2. Nhập thủ công 1 nhân viên");
        System.Console.Write("Chọn phương thức (1-2): ");

        var choice = System.Console.ReadLine()?.Trim();
        var importList = new List<BatchImportEmployeeDto>();

        if (choice == "1")
        {
            var randomSuffix = Random.Shared.Next(100, 999);
            importList.Add(new BatchImportEmployeeDto
            {
                FirstName = "Hòa",
                LastName = "Đặng Dev",
                Email = $"dev.demo{randomSuffix}@company.com",
                Password = "Employee123!",
                Gender = "Male",
                Department = "IT",
                PhoneNumber = $"090{randomSuffix}001",
                IsIntern = false,
                EmployeeType = "Developer",
                Band = 2,
                TechnicalDirection = "C# / ASP.NET Core"
            });

            importList.Add(new BatchImportEmployeeDto
            {
                FirstName = "Yến",
                LastName = "Trịnh QA",
                Email = $"qa.demo{randomSuffix}@company.com",
                Password = "Employee123!",
                Gender = "Female",
                Department = "IT",
                PhoneNumber = $"090{randomSuffix}002",
                IsIntern = false,
                EmployeeType = "QA",
                Band = 3,
                CodingSkillsFlag = true
            });

            importList.Add(new BatchImportEmployeeDto
            {
                FirstName = "Phong",
                LastName = "Lê Manager",
                Email = $"mgr.demo{randomSuffix}@company.com",
                Password = "Manager123!",
                Gender = "Male",
                Department = "HR",
                PhoneNumber = $"090{randomSuffix}003",
                IsIntern = false,
                EmployeeType = "Manager",
                ManagerType = "DepartmentManager"
            });
        }
        else if (choice == "2")
        {
            System.Console.Write("Họ: ");
            var lastName = System.Console.ReadLine()?.Trim() ?? "Nguyễn";
            System.Console.Write("Tên: ");
            var firstName = System.Console.ReadLine()?.Trim() ?? "Văn A";
            System.Console.Write("Email: ");
            var email = System.Console.ReadLine()?.Trim() ?? $"user{Random.Shared.Next(100, 999)}@company.com";
            System.Console.Write("Mật khẩu: ");
            var pass = System.Console.ReadLine()?.Trim() ?? "Employee123!";
            System.Console.Write("Loại nhân viên (Developer/QA/Manager/Employee): ");
            var empType = System.Console.ReadLine()?.Trim() ?? "Employee";

            importList.Add(new BatchImportEmployeeDto
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = pass,
                Gender = "Male",
                Department = "IT",
                PhoneNumber = "0909999999",
                IsIntern = false,
                EmployeeType = empType
            });
        }
        else
        {
            PrintError("Lựa chọn hủy.");
            PressAnyKeyToContinue();
            return;
        }

        try
        {
            PrintInfo($"Đang gửi request POST {_api.BaseUrl}/api/Employees/batch-import ...");
            var result = await _api.BatchImportAsync(importList);

            PrintSuccess($"Import hoàn tất!");
            System.Console.WriteLine($" - Số lượng thành công: {result.SuccessCount}");
            System.Console.WriteLine($" - Số lượng thất bại : {result.FailedCount}");

            if (result.Errors.Count > 0)
            {
                PrintError("Chi tiết lỗi từ Server:");
                foreach (var err in result.Errors)
                {
                    System.Console.WriteLine($"   + {err}");
                }
            }
        }
        catch (Exception ex)
        {
            PrintError($"Import thất bại: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    #endregion

    #region Render Helpers

    private void RenderAttendanceRecordsTable(List<AttendanceRecordDto> records)
    {
        if (records == null || records.Count == 0)
        {
            PrintWarning("Không tìm thấy dữ liệu điểm danh nào.");
            return;
        }

        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine(string.Format("{0,-36} | {1,-20} | {2,-12} | {3,-10} | {4,-19} | {5,-19}",
            "ID BẢN GHI", "NHÂN VIÊN", "PHÒNG BAN", "NGÀY", "GIỜ VÀO", "GIỜ RA"));
        System.Console.WriteLine(new string('-', 125));
        System.Console.ResetColor();

        foreach (var r in records)
        {
            var arrTime = r.ArrivalTime.ToString("HH:mm:ss dd/MM/yyyy");
            var depTime = r.DepartureTime.HasValue ? r.DepartureTime.Value.ToString("HH:mm:ss dd/MM/yyyy") : "Chưa Check-out";

            System.Console.WriteLine(string.Format("{0,-36} | {1,-20} | {2,-12} | {3,-10:dd/MM/yyyy} | {4,-19} | {5,-19}",
                r.Id,
                Truncate(r.EmployeeName, 20),
                Truncate(r.Department, 12),
                r.Date,
                arrTime,
                depTime));
        }

        System.Console.WriteLine(new string('-', 125));
        PrintInfo($"Tổng cộng: {records.Count} bản ghi điểm danh.");
    }

    private void RenderEmployeesTable(List<EmployeeDto> employees)
    {
        if (employees == null || employees.Count == 0)
        {
            PrintWarning("Không tìm thấy nhân viên nào.");
            return;
        }

        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine(string.Format("{0,-36} | {1,-22} | {2,-12} | {3,-15} | {4,-10} | {5,-12} | {6,-8}",
            "ID NHÂN VIÊN", "HỌ VÀ TÊN", "GIỚI TÍNH", "PHÒNG BAN", "VAI TRÒ", "LOẠI NV", "INTERN"));
        System.Console.WriteLine(new string('-', 126));
        System.Console.ResetColor();

        foreach (var emp in employees)
        {
            var fullName = $"{emp.FirstName} {emp.LastName}";
            System.Console.WriteLine(string.Format("{0,-36} | {1,-22} | {2,-12} | {3,-15} | {4,-10} | {5,-12} | {6,-8}",
                emp.Id,
                Truncate(fullName, 22),
                emp.Gender,
                emp.Department,
                emp.Role,
                emp.EmployeeType,
                emp.IsIntern ? "Có" : "Không"));
        }

        System.Console.WriteLine(new string('-', 126));
        PrintInfo($"Tổng cộng: {employees.Count} nhân viên.");
    }

    private void DisplayDemoAccounts()
    {
        PrintHeader("DANH SÁCH TÀI KHOẢN MẪU (SEED DATA)");

        System.Console.WriteLine("1. Admin (Quản trị hệ thống):");
        System.Console.WriteLine("   - Email   : admin@company.com");
        System.Console.WriteLine("   - Mật khẩu: Admin123!");
        System.Console.WriteLine("   - Quyền   : Admin (Toàn quyền)");
        System.Console.WriteLine();

        System.Console.WriteLine("2. General Manager (Giám Đốc):");
        System.Console.WriteLine("   - Email   : gm@company.com");
        System.Console.WriteLine("   - Mật khẩu: Manager123!");
        System.Console.WriteLine("   - Quyền   : GeneralManager");
        System.Console.WriteLine();

        System.Console.WriteLine("3. Department Manager (Trưởng phòng IT):");
        System.Console.WriteLine("   - Email   : deptmanager.it@company.com");
        System.Console.WriteLine("   - Mật khẩu: Manager123!");
        System.Console.WriteLine("   - Quyền   : DepartmentManager (Phòng IT)");
        System.Console.WriteLine();

        System.Console.WriteLine("4. Employee (Developer - Tech Lead):");
        System.Console.WriteLine("   - Email   : lead.dev@company.com");
        System.Console.WriteLine("   - Mật khẩu: Employee123!");
        System.Console.WriteLine("   - Quyền   : Employee");
        System.Console.WriteLine();

        System.Console.WriteLine("5. Employee (Fullstack Developer):");
        System.Console.WriteLine("   - Email   : fullstack.dev@company.com");
        System.Console.WriteLine("   - Mật khẩu: Employee123!");
        System.Console.WriteLine("   - Quyền   : Employee");
        System.Console.WriteLine();

        System.Console.WriteLine("6. Employee (QA Automation):");
        System.Console.WriteLine("   - Email   : automation.qa@company.com");
        System.Console.WriteLine("   - Mật khẩu: Employee123!");
        System.Console.WriteLine("   - Quyền   : Employee");
        System.Console.WriteLine();

        PressAnyKeyToContinue();
    }

    #endregion

    #region Input & Console Utilities

    private (DateTime? StartDate, DateTime? EndDate) ReadOptionalDateRange()
    {
        System.Console.Write("Nhập ngày bắt đầu (yyyy-MM-dd, để trống nếu không lọc): ");
        var startStr = System.Console.ReadLine()?.Trim();
        DateTime? startDate = DateTime.TryParse(startStr, out var sd) ? sd : null;

        System.Console.Write("Nhập ngày kết thúc (yyyy-MM-dd, để trống nếu không lọc): ");
        var endStr = System.Console.ReadLine()?.Trim();
        DateTime? endDate = DateTime.TryParse(endStr, out var ed) ? ed : null;

        return (startDate, endDate);
    }

    private Department? SelectDepartment(string? defaultDeptStr)
    {
        System.Console.WriteLine("Chọn phòng ban:");
        System.Console.WriteLine(" 1. IT");
        System.Console.WriteLine(" 2. HR");
        System.Console.WriteLine(" 3. Finance");
        System.Console.WriteLine(" 4. Sales");

        if (!string.IsNullOrWhiteSpace(defaultDeptStr))
        {
            System.Console.WriteLine($" (Để trống để dùng phòng ban hiện tại của bạn: {defaultDeptStr})");
        }

        System.Console.Write("Lựa chọn (1-4): ");
        var choice = System.Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(choice) && !string.IsNullOrWhiteSpace(defaultDeptStr))
        {
            if (Enum.TryParse<Department>(defaultDeptStr, true, out var defaultDept))
                return defaultDept;
        }

        return choice switch
        {
            "1" => Department.IT,
            "2" => Department.HR,
            "3" => Department.Finance,
            "4" => Department.Sales,
            _ => null
        };
    }

    private static string ReadPassword()
    {
        var password = new StringBuilder();
        while (true)
        {
            var key = System.Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
                break;

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    System.Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                System.Console.Write("*");
            }
        }
        return password.ToString();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
    }

    private static void PressAnyKeyToContinue()
    {
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine("Bấm phím bất kỳ để tiếp tục...");
        System.Console.ResetColor();
        System.Console.ReadKey(intercept: true);
    }

    private static void PrintHeader(string title)
    {
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"=== {title} ===");
        System.Console.ResetColor();
    }

    private static void PrintSuccess(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"[THÀNH CÔNG] {message}");
        System.Console.ResetColor();
    }

    private static void PrintError(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"[LỖI] {message}");
        System.Console.ResetColor();
    }

    private static void PrintWarning(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkYellow;
        System.Console.WriteLine($"[CẢNH BÁO] {message}");
        System.Console.ResetColor();
    }

    private static void PrintInfo(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine($"[THÔNG TIN] {message}");
        System.Console.ResetColor();
    }

    #endregion
}

/// <summary>
/// Alias class 'Console' đại diện cho ConsoleApp tương tác qua HTTP RESTful API.
/// </summary>
public class Console : ConsoleApp
{
    public Console(IConfiguration configuration, ILogger<ConsoleApp> logger) : base(configuration, logger)
    {
    }
}
