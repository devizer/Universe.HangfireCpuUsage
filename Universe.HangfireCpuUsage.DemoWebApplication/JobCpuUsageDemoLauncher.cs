﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using Hangfire;

namespace Universe.HangfireCpuUsage.DemoWebApplication
{
    public class JobCpuUsageDemoLauncher : IHostedService
    {
        IBackgroundJobClient _jobClient;

        public JobCpuUsageDemoLauncher(IBackgroundJobClient jobClient)
        {
            _jobClient = jobClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _jobClient.Enqueue<MyBackgroundServices>(svc => svc.MultiThreadCpuStress(4, 200));
            _jobClient.Enqueue<MyBackgroundServices>(svc => svc.CpuStress(200));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }

    public class MyBackgroundServices
    {
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

        private void LoadCpu(int minDuration, int minCpuUsage, bool needKernelLoad = true)
        {
            Stopwatch sw = Stopwatch.StartNew();
            CpuUsage.CpuUsage firstUsage = CpuUsage.CpuUsage.GetByThread().Value;
            bool isDone = false;
            while (!isDone)
            {
                if (needKernelLoad)
                {
                    IntPtr ptr = Marshal.AllocHGlobal(1 * 1024);
                    Marshal.FreeHGlobal(ptr);
                }

                CpuUsage.CpuUsage nextUsage = CpuUsage.CpuUsage.GetByThread().Value;
                isDone =
                    sw.ElapsedMilliseconds >= minDuration
                    && (nextUsage - firstUsage).TotalMicroSeconds >= minCpuUsage * 1000L;
            }
        }
    }
}
