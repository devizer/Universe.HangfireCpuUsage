using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Universe.HangfireCpuUsage.DemoWebApplication;

public class DemoJobs
{
    public async Task MultiThreadCpuStressThenFall(int threadsCount, int requiredCpuUsage)
    {
        MultiThreadCpuStress(threadsCount, requiredCpuUsage);
        throw new InvalidOperationException("Operation failed on purpose");
    }
    public async Task MultiThreadCpuStress(int threadsCount, int requiredCpuUsage)
    {
        List<Task> tasks = new List<Task>();
        for (int i = 1; i <= threadsCount; i++)
            tasks.Add(Task.Run(() => LoadCpu(1, requiredCpuUsage, true)));

        Task.WaitAll(tasks.ToArray());
    }

    public async Task CpuStress(int requiredCpuUsage)
    {
        await MultiThreadCpuStress(1, requiredCpuUsage);
    }

    public async Task Sleep(int duration, CancellationToken cancellationToken)
    {
        await Task.Delay(duration, cancellationToken);
    }

    private void LoadCpu(int minDuration, int minCpuUsage, bool needKernelLoad = true)
    {
        Stopwatch sw = Stopwatch.StartNew();
        CpuUsage.CpuUsage firstUsage = CpuUsage.CpuUsage.GetByThread().Value;
        bool isDone = false;
        while (!isDone)
        {
            for(int i=1; i <= (needKernelLoad ? 3 : 0); i++)
            {
                IntPtr ptr = Marshal.AllocHGlobal(2 * 1024);
                Marshal.FreeHGlobal(ptr);
            }

            CpuUsage.CpuUsage nextUsage = CpuUsage.CpuUsage.GetByThread().Value;
            isDone =
                sw.ElapsedMilliseconds >= minDuration
                && (nextUsage - firstUsage).TotalMicroSeconds >= minCpuUsage * 1000L;
        }
    }
}