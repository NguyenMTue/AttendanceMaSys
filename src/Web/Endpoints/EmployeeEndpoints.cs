using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.Commands;
using AttendanceMaSys.Application.Employees.DTOs;
using AttendanceMaSys.Application.Employees.Queries;
using AttendanceMaSys.Domain.Enums;
using AttendanceMaSys.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceMaSys.Web.Endpoints;

public class EmployeeEndpoints : IEndpointGroup
{
    public static string RoutePrefix => "/api/Employees";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetEmployees)
            .WithName("GetEmployees")
            .WithSummary("Lấy danh sách nhân viên")
            .RequireAuthorization();

        groupBuilder.MapPost("/batch-import", BatchImport)
            .WithName("BatchImportEmployees")
            .WithSummary("Import hàng loạt nhân viên từ tập tin (Admin)")
            .RequireAuthorization();

        groupBuilder.MapPost("/import/preview", PreviewExcelImport)
            .WithName("PreviewExcelImport")
            .WithSummary("Chạy thử (kiểm tra, xem trước) dữ liệu import từ file Excel (.xlsx)")
            .DisableAntiforgery()
            .RequireAuthorization();

        groupBuilder.MapPost("/import/preview-json", PreviewJsonImport)
            .WithName("PreviewJsonImport")
            .WithSummary("Chạy thử (kiểm tra, xem trước) dữ liệu import từ danh sách JSON")
            .RequireAuthorization();

        groupBuilder.MapPost("/import/confirm", ConfirmExcelImport)
            .WithName("ConfirmExcelImport")
            .WithSummary("Xác nhận lưu danh sách nhân viên đã qua kiểm tra")
            .RequireAuthorization();

        groupBuilder.MapPost("/import/async", StartAsyncExcelImport)
            .WithName("StartAsyncExcelImport")
            .WithSummary("Khởi chạy import bất đồng bộ cho file Excel (.xlsx) kích thước lớn")
            .DisableAntiforgery()
            .RequireAuthorization();

        groupBuilder.MapPost("/import/async-json", StartAsyncJsonImport)
            .WithName("StartAsyncJsonImport")
            .WithSummary("Khởi chạy import bất đồng bộ cho danh sách JSON kích thước lớn")
            .RequireAuthorization();

        groupBuilder.MapGet("/import/jobs/{jobId:guid}", GetImportJobStatus)
            .WithName("GetImportJobStatus")
            .WithSummary("Kiểm tra trạng thái và tiến độ xử lý bất đồng bộ (bao gồm log lỗi)")
            .RequireAuthorization();

        groupBuilder.MapGet("/import/template", DownloadExcelTemplate)
            .WithName("DownloadExcelTemplate")
            .WithSummary("Tải file Excel mẫu (.xlsx) để nhập dữ liệu")
            .RequireAuthorization();
    }

    public static async Task<IResult> GetEmployees(ISender sender, [FromQuery] Department? department)
    {
        var query = new GetEmployeesQuery(department);
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> BatchImport(ISender sender, [FromBody] BatchImportEmployeesCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> PreviewExcelImport(
        ISender sender,
        IExcelImportService excelService,
        IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return TypedResults.BadRequest("Vui lòng cung cấp file Excel (.xlsx) để kiểm tra.");
        }

        using var stream = file.OpenReadStream();
        var employees = excelService.ReadEmployeesFromExcel(stream);

        var command = new PreviewBatchImportCommand(employees);
        var previewResult = await sender.Send(command);
        return TypedResults.Ok(previewResult);
    }

    public static async Task<IResult> PreviewJsonImport(
        ISender sender,
        [FromBody] List<BatchImportEmployeeDto> employees)
    {
        var command = new PreviewBatchImportCommand(employees);
        var previewResult = await sender.Send(command);
        return TypedResults.Ok(previewResult);
    }

    public static async Task<IResult> ConfirmExcelImport(ISender sender, [FromBody] BatchImportEmployeesCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> StartAsyncExcelImport(
        ISender sender,
        IExcelImportService excelService,
        IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return TypedResults.BadRequest("Vui lòng cung cấp file Excel (.xlsx) để xử lý bất đồng bộ.");
        }

        using var stream = file.OpenReadStream();
        var employees = excelService.ReadEmployeesFromExcel(stream);

        var command = new StartAsyncBatchImportCommand(employees);
        var jobId = await sender.Send(command);
        return TypedResults.Ok(new { JobId = jobId, Message = "Đã khởi chạy tác vụ import bất đồng bộ thành công.", TotalRows = employees.Count });
    }

    public static async Task<IResult> StartAsyncJsonImport(
        ISender sender,
        [FromBody] List<BatchImportEmployeeDto> employees)
    {
        var command = new StartAsyncBatchImportCommand(employees);
        var jobId = await sender.Send(command);
        return TypedResults.Ok(new { JobId = jobId, Message = "Đã khởi chạy tác vụ import bất đồng bộ thành công.", TotalRows = employees.Count });
    }

    public static async Task<IResult> GetImportJobStatus(ISender sender, Guid jobId)
    {
        var query = new GetImportJobStatusQuery(jobId);
        var status = await sender.Send(query);
        if (status == null)
        {
            return TypedResults.NotFound(new { Message = $"Không tìm thấy tiến trình import với ID: {jobId}" });
        }
        return TypedResults.Ok(status);
    }

    public static async Task<IResult> DownloadExcelTemplate(ISender sender)
    {
        var query = new GetBatchImportTemplateQuery();
        var fileBytes = await sender.Send(query);
        return TypedResults.File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employees_Import_Template.xlsx");
    }
}
