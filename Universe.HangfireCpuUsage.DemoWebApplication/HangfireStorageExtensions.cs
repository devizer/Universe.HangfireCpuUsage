using System.Diagnostics;
using Hangfire;

namespace Universe.HangfireCpuUsage.DemoWebApplication
{
    public static class HangfireStorageExtensions
    {
        private const string SucceededStateName = "Succeeded";
        private const string FailedStateName = "Failed";
        private const string DeletedStateName = "Deleted";

        public static JobCompletionStatus WaitForJobCompletion(this JobStorage storage, string jobId, int timeoutMilliseconds = 1000)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var monitoring = storage.GetMonitoringApi();
            // IEnumerable<string> hist;
            // string histInfo;
            JobCompletionStatus? ret = null;
            while (true)
            {
                var job = monitoring.JobDetails(jobId);
                if (job == null) return JobCompletionStatus.NotFound;
                // {x.Reason} --> {x.StateName}
                // [ --> Enqueued,  --> Processing, An exception occurred during performance of the job. --> Failed, Retry attempt 1 of 2: Operation failed on purpose --> Scheduled, Triggered by DelayedJobScheduler --> Enqueued,  --> Processing, An exception occurred during performance of the job. --> Failed, Retry attempt 2 of 2: Operation failed on purpose --> Scheduled, Triggered by DelayedJobScheduler --> Enqueued,  --> Processing, An exception occurred during performance of the job. --> Failed, Exceeded the maximum number of retry attempts. --> Deleted]
                // [ --> Enqueued,  --> Processing,  --> Succeeded]
                foreach (var stage in job.History)
                {
                    if (stage.StateName == SucceededStateName) return JobCompletionStatus.Competed;
                    if (stage.StateName == DeletedStateName || stage.StateName == FailedStateName) return JobCompletionStatus.Failed;
                }

                if (timeoutMilliseconds >= 0 && sw.ElapsedMilliseconds >= timeoutMilliseconds)
                {
                    // Console.WriteLine($"Timeout for job {jobId} [{string.Join(" --> ", job.History.Select(x => x.StateName).Reverse())}]");
                    return JobCompletionStatus.Timeout;
                }
                Thread.Sleep(1);
            }

            // throw new InvalidOperationException("Never goes here");
        }

        public enum JobCompletionStatus
        {
            Competed,
            Failed,
            NotFound,
            Timeout,
        }
    }
}
