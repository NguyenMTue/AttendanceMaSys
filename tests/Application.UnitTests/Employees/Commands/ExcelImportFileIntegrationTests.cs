using System.IO;
using System.Threading.Tasks;
using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.Commands;
using AttendanceMaSys.Infrastructure.Services;
using Moq;
using NUnit.Framework;

namespace AttendanceMaSys.Application.UnitTests.Employees.Commands;

[TestFixture]
public class ExcelImportFileIntegrationTests
{
    private ExcelImportService _excelImportService = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;

    [SetUp]
    public void Setup()
    {
        _excelImportService = new ExcelImportService();
        _identityServiceMock = new Mock<IIdentityService>();
        _identityServiceMock
            .Setup(x => x.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
    }

    [Test]
    public async Task Test_Import_Vietnamese_ExcelFile()
    {
        var filePath = @"E:\Tue\MyProject\AttendanceMaSys\FileExcel\Danh_sach_nhan_vien_VI.xlsx";
        Assert.That(File.Exists(filePath), Is.True, $"File not found at: {filePath}");

        using var stream = File.OpenRead(filePath);
        var employees = _excelImportService.ReadEmployeesFromExcel(stream);

        TestContext.Out.WriteLine($"=== SỐ LƯỢNG DÒNG ĐỌC ĐƯỢC TỪ FILE TIẾNG VIỆT: {employees.Count} ===");
        foreach (var emp in employees)
        {
            TestContext.Out.WriteLine($" -> {emp.FirstName} {emp.LastName} | Email: {emp.Email} | Dept: {emp.Department} | Type: {emp.EmployeeType} | Gender: {emp.Gender}");
        }

        var handler = new PreviewBatchImportCommandHandler(_identityServiceMock.Object);
        var preview = await handler.Handle(new PreviewBatchImportCommand(employees), default);

        TestContext.Out.WriteLine($"=== KẾT QUẢ PREVIEW FILE TIẾNG VIỆT ===");
        TestContext.Out.WriteLine($"Tổng số: {preview.TotalRows} | Hợp lệ: {preview.ValidRowsCount} | Lỗi: {preview.InvalidRowsCount} | CanCommit: {preview.CanCommit}");

        foreach (var item in preview.Items)
        {
            TestContext.Out.WriteLine($"Row #{item.RowIndex}: Valid={item.IsValid}, Errors=[{string.Join("; ", item.Errors)}]");
        }

        Assert.That(preview.TotalRows, Is.GreaterThan(0));
        Assert.That(preview.ValidRowsCount, Is.EqualTo(preview.TotalRows), "Tất cả các dòng trong file tiếng Việt phải hợp lệ!");
    }

    [Test]
    public async Task Test_Import_English_ExcelFile()
    {
        var filePath = @"E:\Tue\MyProject\AttendanceMaSys\FileExcel\Employee_List_EN.xlsx";
        Assert.That(File.Exists(filePath), Is.True, $"File not found at: {filePath}");

        using var stream = File.OpenRead(filePath);
        var employees = _excelImportService.ReadEmployeesFromExcel(stream);

        TestContext.Out.WriteLine($"=== SỐ LƯỢNG DÒNG ĐỌC ĐƯỢC TỪ FILE TIẾNG ANH: {employees.Count} ===");
        foreach (var emp in employees)
        {
            TestContext.Out.WriteLine($" -> {emp.FirstName} {emp.LastName} | Email: {emp.Email} | Dept: {emp.Department} | Type: {emp.EmployeeType} | Gender: {emp.Gender}");
        }

        var handler = new PreviewBatchImportCommandHandler(_identityServiceMock.Object);
        var preview = await handler.Handle(new PreviewBatchImportCommand(employees), default);

        TestContext.Out.WriteLine($"=== KẾT QUẢ PREVIEW FILE TIẾNG ANH ===");
        TestContext.Out.WriteLine($"Tổng số: {preview.TotalRows} | Hợp lệ: {preview.ValidRowsCount} | Lỗi: {preview.InvalidRowsCount} | CanCommit: {preview.CanCommit}");

        foreach (var item in preview.Items)
        {
            TestContext.Out.WriteLine($"Row #{item.RowIndex}: Valid={item.IsValid}, Errors=[{string.Join("; ", item.Errors)}]");
        }

        Assert.That(preview.TotalRows, Is.GreaterThan(0));
        Assert.That(preview.ValidRowsCount, Is.EqualTo(preview.TotalRows), "Tất cả các dòng trong file tiếng Anh phải hợp lệ!");
    }
}
