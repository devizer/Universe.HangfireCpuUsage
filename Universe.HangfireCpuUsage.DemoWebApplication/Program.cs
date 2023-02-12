using Hangfire;
using Hangfire.Console;
using Universe.HangfireCpuUsage.DemoWebApplication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHostedService<JobCpuUsageDemoLauncher>();
builder.Services.AddTransient<MyBackgroundServices>();
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
app.UseHangfireDashboard();

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
