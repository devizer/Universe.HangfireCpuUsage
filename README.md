# Universe.HangfireCpuUsage [![Nuget](https://img.shields.io/nuget/v/Universe.HangfireCpuUsage)](https://www.nuget.org/packages/Universe.HangfireCpuUsage)
Hangfire's CPU usage intergration provides cpu usage info for both sync and async jobs

## Minimum OS Requirements
Linux Kernel 2.6.26+, Mac OS 10.9+, Windows Vista+

## Usage: Log to Job's console on dashboard and to an ILogger
```
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
        var logger = loggerFactory.CreateLogger(category);
        logger.LogInformation($"Arguments: {job.FormatArguments()}" +
                              Environment.NewLine +
                              $"Job took {cpuUsage}");
    })
);
```

### Logger output
```
info: Universe.HangfireCpuUsage.DemoWebApplication.MyBackgroundServices.CpuStress[0]
      Arguments: requiredCpuUsage = 200
      Job took 202.50 ms (cpu: 100.3%, 203.12 = 187.50 [user] + 15.62 [kernel], 1 sub-task)
info: Universe.HangfireCpuUsage.DemoWebApplication.MyBackgroundServices.MultiThreadCpuStress[0]
      Arguments: threadsCount = 4, requiredCpuUsage = 200
      Job took 202.49 ms (cpu: 401.2%, 812.50 = 687.50 [user] + 125.00 [kernel], 4 sub-tasks)
```
