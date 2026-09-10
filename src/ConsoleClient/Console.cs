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
            System.Console.WriteLine(" 8. Thay đổi vị trí / Thăng chức nhân viên");
        }

        if (isAdminOrGM)
        {
            System.Console.WriteLine(" 5. Xem lịch sử điểm danh toàn công ty");
            System.Console.WriteLine(" 9. Cho nghỉ việc / Sa thải & Xóa quyền truy cập nhân viên");
        }

        if (isAdmin)
        {
            System.Console.WriteLine(" 7. Import file Excel (.xlsx) - Chạy thử & Bất đồng bộ (Admin)");
        }

        System.Console.WriteLine(" 10. Đăng xuất (Logout)");
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
                if (isManagerOrAdmin) await HandleUpdatePositionAsync();
                else PrintError("Bạn không có quyền truy cập chức năng này.");
                break;
            case "9":
                if (isAdminOrGM) await HandleTerminateEmployeeAsync();
                else PrintError("Bạn không có quyền truy cập chức năng này.");
                break;
            case "10":
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
        PrintHeader("IMPORT NHÂN VIÊN TỪ FILE EXCEL .XLSX (ADMIN)");

        System.Console.WriteLine(" Vui lòng chọn chế độ xử lý:");
        System.Console.WriteLine(" 1. Import từ file Excel (.xlsx) - CHẾ ĐỘ CHẠY THỬ (Kiểm tra, xem trước thông tin rồi mới xác nhận lưu)");
        System.Console.WriteLine(" 2. Import BẤT ĐỒNG BỘ từ file Excel (.xlsx) - Dành cho file KÍCH THƯỚC LỚN & Kiểm tra log lỗi");
        System.Console.WriteLine(" 3. Tải / Tạo file Excel mẫu (.xlsx) tại máy local");
        System.Console.WriteLine(" 4. Chạy thử nhanh với mẫu dữ liệu test (3 nhân viên)");
        System.Console.WriteLine(" 0. Quay lại menu chính");
        System.Console.WriteLine();
        System.Console.Write("Vui lòng chọn (0-4): ");

        var choice = System.Console.ReadLine()?.Trim();
        switch (choice)
        {
            case "1":
                await HandleExcelDryRunImportAsync();
                break;
            case "2":
                await HandleExcelAsyncImportAsync();
                break;
            case "3":
                await HandleDownloadTemplateAsync();
                break;
            case "4":
                await HandleDemoDryRunImportAsync();
                break;
            case "0":
                return;
            default:
                PrintError("Lựa chọn không hợp lệ.");
                PressAnyKeyToContinue();
                break;
        }
    }

    private async Task HandleExcelDryRunImportAsync()
    {
        PrintHeader("CHẾ ĐỘ CHẠY THỬ (PREVIEW / DRY-RUN) TỪ FILE EXCEL");

        System.Console.Write("Nhập đường dẫn tập tin Excel (.xlsx) [Để trống để tự tạo file Excel test mẫu]: ");
        var filePath = System.Console.ReadLine()?.Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = Path.Combine(Directory.GetCurrentDirectory(), "temp_test_import.xlsx");
            PrintInfo($"Tự động tạo tập tin Excel test mẫu tại: {filePath}");
            GenerateSampleExcelFile(filePath, count: 5);
        }

        if (!File.Exists(filePath))
        {
            PrintError($"Không tìm thấy tập tin tại đường dẫn: {filePath}");
            PressAnyKeyToContinue();
            return;
        }

        try
        {
            PrintInfo($"Đang gửi file Excel đến Server để CHẠY THỬ / KIỂM TRA (POST /api/Employees/import/preview)...");
            var preview = await _api.PreviewImportFromExcelAsync(filePath);

            RenderPreviewResultTable(preview);

            if (preview.CanCommit && preview.ValidRowsCount > 0)
            {
                System.Console.WriteLine();
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Write($" Bạn có muốn XÁC NHẬN LƯU {preview.ValidRowsCount} nhân viên hợp lệ vào hệ thống không? (Y/N): ");
                System.Console.ResetColor();

                var confirm = System.Console.ReadLine()?.Trim();
                if (confirm?.Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var validEmployees = preview.Items.Where(x => x.IsValid).Select(x => x.Employee).ToList();
                    PrintInfo("Đang gửi lệnh XÁC NHẬN LƯU đến Server (POST /api/Employees/import/confirm)...");
                    var result = await _api.ConfirmImportAsync(validEmployees);

                    PrintSuccess($"Lưu dữ liệu hoàn tất! Số lượng thành công: {result.SuccessCount}, Thất bại: {result.FailedCount}");
                    if (result.Errors.Count > 0)
                    {
                        PrintError("Chi tiết lỗi khi lưu:");
                        foreach (var err in result.Errors)
                        {
                            System.Console.WriteLine($"   + {err}");
                        }
                    }
                }
                else
                {
                    PrintWarning("Đã hủy thao tác lưu dữ liệu.");
                }
            }
            else
            {
                PrintError("File Excel không có dòng nào hợp lệ để lưu vào hệ thống.");
            }
        }
        catch (Exception ex)
        {
            PrintError($"Chạy thử import thất bại: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    private async Task HandleExcelAsyncImportAsync()
    {
        PrintHeader("IMPORT BẤT ĐỒNG BỘ CHO FILE LỚN & KIỂM TRA LOG LỖI");

        System.Console.Write("Nhập đường dẫn tập tin Excel (.xlsx) [Để trống để tạo file Excel lớn 50 bản ghi mẫu]: ");
        var filePath = System.Console.ReadLine()?.Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = Path.Combine(Directory.GetCurrentDirectory(), "temp_large_test_import.xlsx");
            PrintInfo($"Tự động tạo file Excel 50 bản ghi test lớn (bao gồm cả dòng lỗi cố ý) tại: {filePath}");
            GenerateSampleExcelFile(filePath, count: 50, includeErrors: true);
        }

        if (!File.Exists(filePath))
        {
            PrintError($"Không tìm thấy tập tin tại đường dẫn: {filePath}");
            PressAnyKeyToContinue();
            return;
        }

        try
        {
            PrintInfo("Đang khởi chạy tác vụ import BẤT ĐỒNG BỘ (POST /api/Employees/import/async)...");
            var jobId = await _api.StartAsyncImportFromExcelAsync(filePath);

            PrintSuccess($"Tác vụ bất đồng bộ đã khởi tạo thành công! Job ID: {jobId}");
            PrintInfo("Đang theo dõi tiến độ xử lý bất đồng bộ từ Server theo thời gian thực...\n");

            ImportJobStatusDto? jobStatus = null;
            while (true)
            {
                jobStatus = await _api.GetImportJobStatusAsync(jobId);
                if (jobStatus == null)
                {
                    PrintError("Không thể lấy trạng thái tiến trình.");
                    break;
                }

                System.Console.Write($"\r [TIẾN ĐỘ BẤT ĐỒNG BỘ] Trạng thái: {jobStatus.Status,-10} | Đã xử lý: {jobStatus.ProcessedRows}/{jobStatus.TotalRows} ({jobStatus.ProgressPercentage}%) | Thành công: {jobStatus.SuccessCount} | Lỗi: {jobStatus.FailedCount}   ");

                if (jobStatus.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    jobStatus.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                await Task.Delay(500);
            }

            System.Console.WriteLine("\n");
            PrintHeader("KẾT QUẢ XỬ LÝ BẤT ĐỒNG BỘ");
            if (jobStatus != null)
            {
                System.Console.WriteLine($" - Job ID      : {jobStatus.JobId}");
                System.Console.WriteLine($" - Trạng thái  : {jobStatus.Status}");
                System.Console.WriteLine($" - Tổng số dòng: {jobStatus.TotalRows}");
                System.Console.WriteLine($" - Thành công  : {jobStatus.SuccessCount}");
                System.Console.WriteLine($" - Thất bại    : {jobStatus.FailedCount}");

                if (jobStatus.Errors.Count > 0)
                {
                    System.Console.WriteLine();
                    PrintError($"DANH SÁCH {jobStatus.Errors.Count} LỖI PHÁT HIỆN TRONG QUÁ TRÌNH XỬ LÝ BẤT ĐỒNG BỘ:");
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine(string.Format("{0,-8} | {1,-30} | {2}", "DÒNG #", "EMAIL TÀI KHOẢN", "CHI TIẾT LỖI"));
                    System.Console.WriteLine(new string('-', 100));
                    System.Console.ResetColor();

                    foreach (var err in jobStatus.Errors)
                    {
                        System.Console.WriteLine(string.Format("{0,-8} | {1,-30} | {2}", err.RowIndex, Truncate(err.Email, 30), err.ErrorMessage));
                    }
                    System.Console.WriteLine(new string('-', 100));
                }
                else
                {
                    PrintSuccess("Không có lỗi bất đồng bộ nào phát sinh!");
                }
            }
        }
        catch (Exception ex)
        {
            PrintError($"Import bất đồng bộ thất bại: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    private async Task HandleDownloadTemplateAsync()
    {
        PrintHeader("TẢI / ĐỊNH DẠNG FILE EXCEL MẪU (.XLSX)");

        try
        {
            PrintInfo("Đang tải file Excel mẫu từ Server (GET /api/Employees/import/template)...");
            var bytes = await _api.DownloadExcelTemplateAsync();

            var targetPath = Path.Combine(Directory.GetCurrentDirectory(), "Employees_Import_Template.xlsx");
            await File.WriteAllBytesAsync(targetPath, bytes);

            PrintSuccess($"Đã tạo file Excel mẫu thành công tại đường dẫn:\n -> {targetPath}");
            PrintInfo("Bạn có thể mở file trên bằng Microsoft Excel hoặc Excel Editor để điền dữ liệu.");
        }
        catch (Exception ex)
        {
            PrintError($"Lỗi khi tải mẫu Excel: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    private async Task HandleDemoDryRunImportAsync()
    {
        PrintHeader("CHẠY THỬ & XEM TRƯỚC VỚI DỮ LIỆU DEMO (3 NHÂN VIÊN)");

        var randomSuffix = Random.Shared.Next(100, 999);
        var demoList = new List<BatchImportEmployeeDto>
        {
            new BatchImportEmployeeDto
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
            },
            new BatchImportEmployeeDto
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
            },
            new BatchImportEmployeeDto
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
            }
        };

        try
        {
            PrintInfo("Đang gửi danh sách demo đến Server để CHẠY THỬ / PREVIEW...");
            var preview = await _api.PreviewImportFromListAsync(demoList);

            RenderPreviewResultTable(preview);

            if (preview.CanCommit)
            {
                System.Console.Write("\nXác nhận LƯU danh sách hợp lệ này vào hệ thống? (Y/N): ");
                var confirm = System.Console.ReadLine()?.Trim();
                if (confirm?.Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var validList = preview.Items.Where(x => x.IsValid).Select(x => x.Employee).ToList();
                    var result = await _api.ConfirmImportAsync(validList);
                    PrintSuccess($"Lưu thành công: {result.SuccessCount}, Thất bại: {result.FailedCount}");
                }
            }
        }
        catch (Exception ex)
        {
            PrintError($"Chạy thử thất bại: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    private static void RenderPreviewResultTable(BatchImportPreviewDto preview)
    {
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine($"=== KẾT QUẢ CHẠY THỬ (PREVIEW) KHỔNG THAY ĐỔI CSDL ===");
        System.Console.ResetColor();

        System.Console.WriteLine($" - Tổng số bản ghi : {preview.TotalRows}");
        System.Console.WriteLine($" - Bản ghi hợp lệ  : {preview.ValidRowsCount}");
        System.Console.WriteLine($" - Bản ghi có lỗi  : {preview.InvalidRowsCount}");
        System.Console.WriteLine();

        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine(string.Format("{0,-6} | {1,-20} | {2,-28} | {3,-10} | {4,-10} | {5,-12} | {6}",
            "DÒNG #", "HỌ VÀ TÊN", "EMAIL TÀI KHOẢN", "PHÒNG BAN", "LOẠI NV", "TRẠNG THÁI", "LỖI PHÁT HIỆN"));
        System.Console.WriteLine(new string('-', 120));
        System.Console.ResetColor();

        foreach (var item in preview.Items)
        {
            var fullName = $"{item.Employee.FirstName} {item.Employee.LastName}";
            var statusStr = item.IsValid ? "HỢP LỆ" : "LỖI";

            if (item.IsValid) System.Console.ForegroundColor = ConsoleColor.Green;
            else System.Console.ForegroundColor = ConsoleColor.Red;

            var errText = item.IsValid ? "Không có lỗi" : string.Join("; ", item.Errors);

            System.Console.WriteLine(string.Format("{0,-6} | {1,-20} | {2,-28} | {3,-10} | {4,-10} | {5,-12} | {6}",
                item.RowIndex,
                Truncate(fullName, 20),
                Truncate(item.Employee.Email, 28),
                item.Employee.Department,
                item.Employee.EmployeeType,
                statusStr,
                errText));
        }

        System.Console.ResetColor();
        System.Console.WriteLine(new string('-', 120));
    }

    private static void GenerateSampleExcelFile(string filePath, int count = 5, bool includeErrors = false)
    {
        var list = new List<BatchImportEmployeeDto>();
        var rand = Random.Shared;

        for (int i = 1; i <= count; i++)
        {
            var isErrorRow = includeErrors && (i % 7 == 0 || i % 13 == 0);
            var isDuplicateRow = includeErrors && (i == 10 || i == 20);

            string email;
            if (isDuplicateRow)
            {
                email = "admin@company.com"; // Duplicated in DB intentionally to test error check
            }
            else if (isErrorRow)
            {
                email = "email_invalid_format"; // Bad format email
            }
            else
            {
                email = $"test.user{rand.Next(1000, 9999)}_{i}@company.com";
            }

            list.Add(new BatchImportEmployeeDto
            {
                FirstName = $"Nhân Viên {i}",
                LastName = "Nguyễn",
                Email = email,
                Password = isErrorRow ? "123" : "Employee123!",
                Gender = i % 2 == 0 ? "Female" : "Male",
                Department = i % 3 == 0 ? "HR" : "IT",
                PhoneNumber = $"090{rand.Next(1000000, 9999999)}",
                IsIntern = i % 5 == 0,
                EmployeeType = i % 4 == 0 ? "QA" : (i % 3 == 0 ? "Manager" : "Developer"),
                Band = 2,
                TechnicalDirection = "C# / ASP.NET Core",
                ManagerType = "DepartmentManager"
            });
        }

        using var memoryStream = new MemoryStream();
        MiniExcelLibs.MiniExcel.SaveAs(memoryStream, list);
        File.WriteAllBytes(filePath, memoryStream.ToArray());
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
        System.Console.WriteLine(string.Format("{0,-36} | {1,-20} | {2,-10} | {3,-12} | {4,-18} | {5,-12} | {6,-12}",
            "ID NHÂN VIÊN", "HỌ VÀ TÊN", "GIỚI TÍNH", "PHÒNG BAN", "VAI TRÒ", "LOẠI NV", "TRẠNG THÁI"));
        System.Console.WriteLine(new string('-', 130));
        System.Console.ResetColor();

        foreach (var emp in employees)
        {
            var fullName = $"{emp.FirstName} {emp.LastName}";
            var statusStr = emp.IsActive ? "Đang làm việc" : "Đã nghỉ việc";

            if (emp.IsActive) System.Console.ForegroundColor = ConsoleColor.White;
            else System.Console.ForegroundColor = ConsoleColor.DarkGray;

            System.Console.WriteLine(string.Format("{0,-36} | {1,-20} | {2,-10} | {3,-12} | {4,-18} | {5,-12} | {6,-12}",
                emp.Id,
                Truncate(fullName, 20),
                emp.Gender,
                emp.Department,
                emp.Role,
                emp.EmployeeType,
                statusStr));
        }

        System.Console.ResetColor();
        System.Console.WriteLine(new string('-', 130));
        PrintInfo($"Tổng cộng: {employees.Count} nhân viên.");
    }

    public async Task HandleUpdatePositionAsync()
    {
        PrintHeader("THAY ĐỔI VỊ TRÍ / PHÒNG BAN / THĂNG CHỨC NHÂN VIÊN");

        var employees = await _api.GetEmployeesAsync(null);
        RenderEmployeesTable(employees);

        System.Console.Write("\nNhập ID nhân viên cần thay đổi vị trí / thăng chức (Guid): ");
        var empIdStr = System.Console.ReadLine()?.Trim();
        if (!Guid.TryParse(empIdStr, out var empId))
        {
            PrintError("ID nhân viên không hợp lệ.");
            PressAnyKeyToContinue();
            return;
        }

        var emp = employees.FirstOrDefault(e => e.Id == empId);
        if (emp == null)
        {
            PrintError("Không tìm thấy nhân viên trong danh sách.");
            PressAnyKeyToContinue();
            return;
        }

        PrintInfo($"Đang cập nhật vị trí cho nhân viên: {emp.FirstName} {emp.LastName} (Hiện tại: Phòng {emp.Department}, Vai trò {emp.Role})");

        var dept = SelectDepartment(emp.Department);

        System.Console.WriteLine("Chọn vai trò / thăng chức mới (để trống nếu giữ nguyên):");
        System.Console.WriteLine(" 1. Employee (Nhân viên)");
        System.Console.WriteLine(" 2. DepartmentManager (Trưởng phòng)");
        System.Console.WriteLine(" 3. GeneralManager (Giám đốc)");
        System.Console.Write("Chọn (1-3, để trống nếu không đổi): ");
        var roleOpt = System.Console.ReadLine()?.Trim();
        RoleEnum? newRole = roleOpt switch
        {
            "1" => RoleEnum.Employee,
            "2" => RoleEnum.DepartmentManager,
            "3" => RoleEnum.GeneralManager,
            _ => null
        };

        System.Console.Write("Nhập Cấp độ / Band mới (số từ 1-10, để trống nếu không đổi): ");
        var bandStr = System.Console.ReadLine()?.Trim();
        int? newBand = int.TryParse(bandStr, out var b) ? b : null;

        System.Console.Write("Nhập Hướng kỹ thuật mới (dành cho Developer, để trống nếu không đổi): ");
        var techDir = System.Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(techDir)) techDir = null;

        try
        {
            PrintInfo("Đang gửi yêu cầu cập nhật vị trí đến Server...");
            var updated = await _api.UpdateEmployeePositionAsync(empId, dept, newRole, emp.EmployeeType, newBand, techDir, null, newRole);

            PrintSuccess($"Cập nhật thành công! Nhân viên {updated.FirstName} {updated.LastName} hiện thuộc phòng: {updated.Department}, Vai trò: {updated.Role}.");
        }
        catch (Exception ex)
        {
            PrintError($"Cập nhật vị trí thất bại: {ex.Message}");
        }

        PressAnyKeyToContinue();
    }

    public async Task HandleTerminateEmployeeAsync()
    {
        PrintHeader("CHO NGHỈ VIỆC / SA THẢI & XÓA QUYỀN TRUY CẬP NHÂN VIÊN");

        var employees = await _api.GetEmployeesAsync(null);
        RenderEmployeesTable(employees);

        System.Console.Write("\nNhập ID nhân viên cần cho nghỉ việc / sa thải (Guid): ");
        var empIdStr = System.Console.ReadLine()?.Trim();
        if (!Guid.TryParse(empIdStr, out var empId))
        {
            PrintError("ID nhân viên không hợp lệ.");
            PressAnyKeyToContinue();
            return;
        }

        var emp = employees.FirstOrDefault(e => e.Id == empId);
        if (emp == null)
        {
            PrintError("Không tìm thấy nhân viên.");
            PressAnyKeyToContinue();
            return;
        }

        System.Console.Write($"Lý do sa thải / cho nghỉ việc đối với {emp.FirstName} {emp.LastName}: ");
        var reason = System.Console.ReadLine()?.Trim();

        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.Write($"\n[CẢNH BÁO] Bạn có CHẮC CHẮN muốn cho nhân viên '{emp.FirstName} {emp.LastName}' nghỉ việc và XÓA QUYỀN TRUY CẬP hệ thống không? (Y/N): ");
        System.Console.ResetColor();

        var confirm = System.Console.ReadLine()?.Trim();
        if (confirm?.Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                PrintInfo("Đang gửi lệnh cho nghỉ việc & vô hiệu hóa tài khoản đến Server...");
                await _api.TerminateEmployeeAsync(empId, reason);

                PrintSuccess($"Đã xử lý cho nghỉ việc thành công! Quyền truy cập và tài khoản của {emp.FirstName} {emp.LastName} đã bị vô hiệu hóa hoàn toàn.");
            }
            catch (Exception ex)
            {
                PrintError($"Thao tác thất bại: {ex.Message}");
            }
        }
        else
        {
            PrintWarning("Hủy thao tác cho nghỉ việc.");
        }

        PressAnyKeyToContinue();
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
