# Universe.HangfireCpuUsage ![Nuget](https://img.shields.io/nuget/v/Universe.HangfireCpuUsage)
Hangfire's CPU usage intergration provides cpu usage info for both sync and async jobs

## Minimum OS Requirements
Linux Kernel 2.6.26+, Mac OS 10.9+, Windows Vista+

## Usage: Log to Job's console on dashboard
```
public void ConfigureServices(IServiceCollection services)
{
  services.AddHangfire(configuration => configuration
    // Apply Console filter BEFORE CPU Usage
    .UseConsole()
    .UseFilter(new CpuUsageJobFilter((context, cpuUsage) =>
    {
        context.WriteLine($"Job took {cpuUsage}");
    }))
    ...
}
```

## Usage: Log to the job's ILogger
```
p
public void ConfigureServices(IServiceCollection services)
{
    // todo
    ...
}
```