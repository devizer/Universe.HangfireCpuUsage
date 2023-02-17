### Universe.HangfireCpuUsage package
Hangfire's CPU usage intergration provides cpu usage info for both sync and async jobs

### Minimum OS Requirements
Linux Kernel 2.6.26+, Mac OS 10.9+, Windows Vista+

### Usage
Configuration example: log cpu usage to the dashboard console, and log  the same to an ILogger
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



Take for example [3 jobs](https://github.com/devizer/Universe.HangfireCpuUsage/blob/main/Universe.HangfireCpuUsage.DemoWebApplication/DemoJobs.cs) with the above configuration
```css
public async Task MultiThreadCpuStress(int threadsCount, int requiredCpuUsage) { … }
public async Task CpuStress(int requiredCpuUsage) { … }
public async Task Sleep(int duration) { … }
```



Logger output is:
```yaml
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


Dashboard Screen is:
![Dashboard Screenshot](https://raw.githubusercontent.com/devizer/Universe.HangfireCpuUsage/main/Images/Hangfire.CpuUsage.Dashboard.png)
