using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Hangfire.Common;
using Hangfire.Server;
using Universe.CpuUsage;

namespace Universe.HangfireCpuUsage
{
    public class CpuUsageJobFilter :
        JobFilterAttribute,
        IServerFilter
    {
        private readonly Action<PerformedContext, JobCpuUsage> Notify;

        public CpuUsageJobFilter(Action<PerformedContext, JobCpuUsage> notify)
        {
            Notify = notify;
        }

        private class JobState
        {
            public Stopwatch StartAt;

            // Sync task
            public CpuUsage.CpuUsage CpuUsageOnStart;

            public int StartThreadId;

            // Async task
            public CpuUsageAsyncWatcher CpuUsageWatcher;
        }

        // static: DI, non static: attribute
        private /* static */ ConcurrentDictionary<string, JobState> _ProcessingJobs = new();


        public void OnPerforming(PerformingContext context)
        {
            var jobIs = context.BackgroundJob.Id;
            if (!string.IsNullOrEmpty(jobIs))
            {
                var cpuUsage = CpuUsage.CpuUsage.GetByThread();
                if (cpuUsage.HasValue)
                {
                    _ProcessingJobs[jobIs] = new JobState()
                    {
                        CpuUsageOnStart = cpuUsage.Value,
                        StartAt = Stopwatch.StartNew(),
                        StartThreadId = Thread.CurrentThread.ManagedThreadId,
                        CpuUsageWatcher = new CpuUsageAsyncWatcher(),
                    };
                }
            }
        }

        public void OnPerformed(PerformedContext context)
        {

            JobCpuUsage data = new JobCpuUsage();

            string cpuUsageAsDebug = null;
            var jobIs = context.BackgroundJob.Id;
            if (!string.IsNullOrEmpty(jobIs))
            {
                if (_ProcessingJobs.TryRemove(jobIs, out var state))
                {
                    state.CpuUsageWatcher.Stop();
                    CpuUsage.CpuUsage? deltaSync = null;
                    var threadId = Thread.CurrentThread.ManagedThreadId;
                    if (threadId == state.StartThreadId)
                    {
                        CpuUsage.CpuUsage onEnd = CpuUsage.CpuUsage.GetByThread().GetValueOrDefault();
                        CpuUsage.CpuUsage onStart = state.CpuUsageOnStart;
                        deltaSync = onEnd - onStart;
                    }

                    double elapsed = state.StartAt.ElapsedTicks * 1000d / Stopwatch.Frequency;
                    data.Duration = elapsed;

                    CpuUsage.CpuUsage? deltaTotal = deltaSync;
                    var totalSubTasks = state.CpuUsageWatcher.Totals;
                    deltaTotal += totalSubTasks.GetSummaryCpuUsage();
                    data.SubTaskCount = totalSubTasks.Count;

                    if (deltaTotal.HasValue)
                    {
                        var delta = deltaTotal.Value;
                        double user = data.UserTime = delta.UserUsage.TotalMicroSeconds / 1000d;
                        double kernel = data.KernelTime = delta.KernelUsage.TotalMicroSeconds / 1000d;
                        double perCents = (user + kernel) / elapsed;
                        string subTaskCount = null;
                        if (totalSubTasks.Count == 1)
                            subTaskCount = ", 1 sub-task";
                        else if (totalSubTasks.Count > 1)
                            subTaskCount = $", {totalSubTasks.Count} sub-tasks";

                        cpuUsageAsDebug = $"{elapsed:n2} ms (cpu: {perCents * 100:n1}%, {user + kernel:n2} = {user:n2} [user] + {kernel:n2} [kernel]{subTaskCount})";
                        data.InfoMessage = cpuUsageAsDebug;
                    }
                }

            }

            var copy = Notify;
            if (copy != null)
                copy(context, data);

            /*
                // if (context.BackgroundJob.Job.Method.Name.IndexOf("Async") >= 0 /*&& Debugger.IsAttached#1#)  Debugger.Break();
                ConsoleExtensions.WriteLine(context, $"Job '{context.BackgroundJob.Job.Type.Name}.{context.BackgroundJob.Job.Method.Name}' has been performed.{Environment.NewLine}{cpuUsageAsDebug}");
    
                var jobKey = $"'{context.BackgroundJob.Job.Type.Name}.{context.BackgroundJob.Job.Method.Name}'";
                Logger.LogInformation(context.BackgroundJob.Id, $"Job '{context.BackgroundJob.Id}' has been performed. '{context.BackgroundJob.Job.Type.Name}.{context.BackgroundJob.Job.Method.Name}' took {cpuUsageAsDebug}");
            */
        }
    }
}
