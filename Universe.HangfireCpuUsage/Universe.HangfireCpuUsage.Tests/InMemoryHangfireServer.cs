using System.Diagnostics;
using Hangfire;
using Hangfire.InMemory;

namespace Universe.HangfireCpuUsage.Tests
{
    public class InMemoryHangfireServer : IDisposable
    {
        public JobStorage Storage { get; }
        public IBackgroundJobClient Client { get; }
        public BackgroundJobServer Server { get; }

        public InMemoryHangfireServer()
        {
            GlobalConfiguration.Configuration.UseColouredConsoleLogProvider();
            Storage = new InMemoryStorage(new InMemoryStorageOptions()
            {
                DisableJobSerialization = false,
            });

            Client = new BackgroundJobClient(Storage);
            Server = new BackgroundJobServer(Storage);
        }

        public WaitForStatus WaitForJobsCount(int expectedJobs, int timeoutMilliseconds = 1000)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var monitoring = Storage.GetMonitoringApi();

            do
            {
                var stat = monitoring.GetStatistics();
                if (stat.Failed + stat.Succeeded >= expectedJobs) return new WaitForStatus()
                {
                    Success = true,
                    Status = $"Wait for {expectedJobs} expected job(s) successfully completed in {sw.ElapsedMilliseconds:n0} milliseconds"
                };

                Thread.Sleep(0);
            }while(timeoutMilliseconds < 0 || sw.ElapsedMilliseconds < timeoutMilliseconds);

            return new WaitForStatus()
            {
                Success = false,
                Status = $"Waiting for {expectedJobs} expected job(s) cancelled by timeout {sw.ElapsedMilliseconds:n0} milliseconds"
            };
        }

        public class WaitForStatus
        {
            public bool Success { get; set; }
            public string Status { get; set; }

            public override string ToString()
            {
                return Status;
            }
        }

        public void Dispose()
        {
            Server.Dispose();
        }

    }
}