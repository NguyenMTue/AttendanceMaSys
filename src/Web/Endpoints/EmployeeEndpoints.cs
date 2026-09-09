using AttendanceMaSys.Application.Employees.Commands;
using AttendanceMaSys.Application.Employees.Queries;
using AttendanceMaSys.Domain.Enums;
using AttendanceMaSys.Web.Infrastructure;
using MediatR;
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
}
