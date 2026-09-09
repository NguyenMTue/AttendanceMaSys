using AttendanceMaSys.Application.Attendance.Commands;
using AttendanceMaSys.Application.Attendance.Queries;
using AttendanceMaSys.Domain.Enums;
using AttendanceMaSys.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceMaSys.Web.Endpoints;

public class AttendanceEndpoints : IEndpointGroup
{
    public static string RoutePrefix => "/api/Attendance";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost("/check-in", CheckIn)
            .WithName("CheckIn")
            .WithSummary("Điểm danh vào ca (Check-in)")
            .RequireAuthorization();

        groupBuilder.MapPost("/check-out", CheckOut)
            .WithName("CheckOut")
            .WithSummary("Điểm danh ra ca (Check-out)")
            .RequireAuthorization();

        groupBuilder.MapGet("/history/personal", GetPersonalHistory)
            .WithName("GetPersonalHistory")
            .WithSummary("Xem lịch sử điểm danh cá nhân")
            .RequireAuthorization();

        groupBuilder.MapGet("/history/department", GetDepartmentHistory)
            .WithName("GetDepartmentHistory")
            .WithSummary("Xem lịch sử điểm danh phòng ban")
            .RequireAuthorization();

        groupBuilder.MapGet("/history/all", GetAllHistory)
            .WithName("GetAllHistory")
            .WithSummary("Xem lịch sử điểm danh toàn công ty")
            .RequireAuthorization();
    }

    public static async Task<IResult> CheckIn(ISender sender, [FromBody] CheckInCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> CheckOut(ISender sender, [FromBody] CheckOutCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> GetPersonalHistory(
        ISender sender,
        [FromQuery] Guid employeeId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var query = new GetPersonalAttendanceHistoryQuery(employeeId, startDate, endDate);
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> GetDepartmentHistory(
        ISender sender,
        [FromQuery] Department department,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var query = new GetDepartmentAttendanceHistoryQuery(department, startDate, endDate);
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> GetAllHistory(
        ISender sender,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var query = new GetAllAttendanceHistoryQuery(startDate, endDate);
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }
}
