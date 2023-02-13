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
            // jit
            _jobClient.Enqueue<DemoJobs>(svc => svc.MultiThreadCpuStress(2, 1));
            _jobClient.Enqueue<DemoJobs>(svc => svc.CpuStress(1));
            _jobClient.Enqueue<DemoJobs>(svc => svc.Sleep(1));
            await Task.Delay(500);

            _jobClient.Enqueue<DemoJobs>(svc => svc.MultiThreadCpuStress(4, 200));
            _jobClient.Enqueue<DemoJobs>(svc => svc.CpuStress(400));
            _jobClient.Enqueue<DemoJobs>(svc => svc.Sleep(600));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}
