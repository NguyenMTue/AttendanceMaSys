using AttendanceMaSys.Application.Auth.Commands;
using AttendanceMaSys.Application.Auth.DTOs;
using AttendanceMaSys.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceMaSys.Web.Endpoints;

public class AuthEndpoints : IEndpointGroup
{
    public static string RoutePrefix => "/api/Auth";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Đăng nhập hệ thống")
            .WithDescription("Xác thực người dùng và trả về JWT Token cùng thông tin phân quyền.")
            .AllowAnonymous();
    }

    public static async Task<IResult> Login(ISender sender, [FromBody] LoginCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }
}
