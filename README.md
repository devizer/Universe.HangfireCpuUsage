# Universe.HangfireCpuUsage [![Nuget](https://img.shields.io/nuget/v/Universe.HangfireCpuUsage)](https://www.nuget.org/packages/Universe.HangfireCpuUsage)
Hangfire's CPU usage intergration provides cpu usage info for both sync and async jobs

## Minimum OS Requirements
Linux Kernel 2.6.26+, Mac OS 10.9+, Windows Vista+

## Usage: Log to both dashboard console and to an ILogger
```csharp
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

#### Take for example [3 jobs](Universe.HangfireCpuUsage.DemoWebApplication/DemoJobs.cs)
```csharp
public async Task MultiThreadCpuStress(int threadsCount, int requiredCpuUsage) { … }
public async Task CpuStress(int requiredCpuUsage) { … }
public async Task Sleep(int duration) { … }
```

#### Logger output is
```css
info: Universe.HangfireCpuUsage.DemoWebApplication.MyJobs.MultiThreadCpuStress[0]
      Arguments: MultiThreadCpuStress(threadsCount = 4, requiredCpuUsage = 200)
      Job took 222.55 ms (cpu: 372.1%, 828.12 = 765.62 [user] + 62.50 [kernel], 4 sub-tasks)
info: Universe.HangfireCpuUsage.DemoWebApplication.MyJobs.CpuStress[0]
      Arguments: CpuStress(requiredCpuUsage = 400)
      Job took 414.29 ms (cpu: 98.1%, 406.25 = 328.12 [user] + 78.12 [kernel], 1 sub-task)
info: Universe.HangfireCpuUsage.DemoWebApplication.MyJobs.Sleep[0]
      Arguments: Sleep(duration = 600)
      Job took 606.12 ms (cpu: 0.0%, 0.00 = 0.00 [user] + 0.00 [kernel], 2 sub-tasks)
```

#### Dashboard Screen
<img src="https://github.com/devizer/Universe.HangfireCpuUsage/raw/main/Images/Hangfire.CpuUsage.Dashboard.png" width="1282px" Alt="Job CPU Usage dashboard screen" Title="Job CPU Usage dashboard screen">
