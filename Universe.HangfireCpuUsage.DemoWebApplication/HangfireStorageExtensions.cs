using System.Diagnostics;
using Hangfire;
using Hangfire.Common;

namespace Universe.HangfireCpuUsage.DemoWebApplication
{
    public static class HangfireStorageExtensions
    {
        public static bool WaitForComplete(this JobStorage storage, string jobId, int timeoutMilliseconds = 1000)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var monitoring = storage.GetMonitoringApi();
            string[] hist;
            string histInfo;
            bool ret = false;
            while (true)
            {
                var job = monitoring.JobDetails(jobId);
                hist = job.History.Select(x => $"{x.Reason} --> {x.StateName}").ToArray();
                histInfo = string.Join(", ", hist);
                bool isFinished = job.History.Any(x => x.Reason == "Completed2");
                if (timeoutMilliseconds >= 0 && sw.ElapsedMilliseconds >= timeoutMilliseconds) break;
                Thread.Sleep(1);
            }

            throw new NotImplementedException("Not used");
            return ret;
        }
    }
}
