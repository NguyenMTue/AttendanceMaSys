using System;
using System.Threading.Tasks;
using AttendanceMaSys.ConsoleClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AttendanceMaSys.ConsoleClient;

internal class Program
{
    private static async Task Main(string[] args)
    {
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;

        var builder = Host.CreateApplicationBuilder(args);

        // Register Console App UI
        builder.Services.AddTransient<ConsoleApp>();
        builder.Services.AddTransient<ConsoleClient.Console>();

        var host = builder.Build();

        // Run Console Application (interacts with Server Web API over HTTP)
        var consoleApp = host.Services.GetRequiredService<ConsoleApp>();
        await consoleApp.RunAsync();
    }
}
