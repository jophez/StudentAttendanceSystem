using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudentAttendanceSystem.Data;
using StudentAttendanceSystem.Service;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Student Attendance Service";
});

builder.Services.AddSingleton<DatabaseConnection>(provider =>
    new DatabaseConnection(DatabaseConnection.GetDefaultConnectionString()));

builder.Services.AddHostedService<AttendanceDisplayService>();

var host = builder.Build();
host.Run();