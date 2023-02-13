using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Universe.HangfireCpuUsage.DemoWebApplication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHostedService<JobCpuUsageDemoLauncher>();
builder.Services.AddTransient<DemoJobs>();
builder.Services.AddHangfire(configuration => configuration
    .UseInMemoryStorage()
    .UseConsole()
    .AddCpuUsageHandler((context, cpuUsage) =>
    {
        // Write to hangfire console output
        context.WriteLine($"Job took {cpuUsage}");
    })
    .AddCpuUsageHandler((context, cpuUsage) =>
    {
        // Write the same to "Type.Method" logger
        var job = context.BackgroundJob.Job;
        var category = job.Type + "." + job.Method.Name;
        var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(category);
        logger.LogInformation($"Arguments: {context.BackgroundJob.Job.FormatArguments()}" +
                              Environment.NewLine +
                              $"Job took {cpuUsage}");
    })
);

builder.Services.AddHangfireServer();


var app = builder.Build();
app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    Authorization = new[] { new AllowAllAuthorizationFilter() }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

public class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
