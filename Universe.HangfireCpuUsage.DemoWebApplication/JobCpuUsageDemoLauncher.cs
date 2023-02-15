using Hangfire;

namespace Universe.HangfireCpuUsage.DemoWebApplication
{
    public class JobCpuUsageDemoLauncher : IHostedService
    {
        IBackgroundJobClient _jobClient;
        private JobStorage _jobStorage;

        public JobCpuUsageDemoLauncher(IBackgroundJobClient jobClient, JobStorage jobStorage)
        {
            _jobClient = jobClient;
            _jobStorage = jobStorage;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // jit
            _jobClient.Enqueue<DemoJobs>(svc => svc.MultiThreadCpuStressThenFall(2, 1));
            _jobClient.Enqueue<DemoJobs>(svc => svc.MultiThreadCpuStress(2, 1));
            var j1 = _jobClient.Enqueue<DemoJobs>(svc => svc.CpuStress(1));
            _jobClient.Enqueue<DemoJobs>(svc => svc.Sleep(1, CancellationToken.None));
            await Task.Delay(500);

            _jobClient.Enqueue<DemoJobs>(svc => svc.MultiThreadCpuStress(4, 200));
            _jobClient.Enqueue<DemoJobs>(svc => svc.CpuStress(400));
            _jobClient.Enqueue<DemoJobs>(svc => svc.Sleep(600, CancellationToken.None));
            var j2 = _jobClient.Enqueue<DemoJobs>(svc => svc.MultiThreadCpuStressThenFall(4, 200));
            _jobClient.Enqueue<DemoJobs>(svc => svc.Sleep(10_000_000, CancellationToken.None));

            Task.Run(() => { Console.WriteLine($"[Status] no_such_job: {_jobStorage.WaitForJobCompletion("no_such_job", timeoutMilliseconds: 2_000)}"); });
            Task.Run(() => { Console.WriteLine($"[Status] normal job: {_jobStorage.WaitForJobCompletion(j1, timeoutMilliseconds: 10_000)}"); });
            Task.Run(() => { Console.WriteLine($"[Status] failed job: {_jobStorage.WaitForJobCompletion(j2, timeoutMilliseconds: 10_000)}"); });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}
