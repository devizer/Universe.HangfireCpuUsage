using Hangfire;
using NUnit.Framework;
using Universe.NUnitTests;

namespace Universe.HangfireCpuUsage.Tests
{
    public class JobCpuUsageTests : NUnitTestsBase
    {
        [Test]
        public void Test1()
        {
            const int expectedCpuUsage = 777;
            int actualCpuUsage = -1;
            bool isNotified = false;
            var cpuUsageJobFilter = new CpuUsageJobFilter((context, usage) =>
            {
                var job = context.BackgroundJob;
                Console.WriteLine($"Job {job.Id} '{job.Job.Type.Name}.{job.Job.Method.Name}' finished.{Environment.NewLine}{usage}");
                actualCpuUsage = (int)(usage.KernelTime + usage.UserTime);
                isNotified = true;
            });

            GlobalJobFilters.Filters.Add(cpuUsageJobFilter);
            OnDispose("Clean Filters", () => GlobalJobFilters.Filters.Add(cpuUsageJobFilter), TestDisposeOptions.Default);
            using var hangfire = new InMemoryHangfireServer();
            hangfire.Client.Enqueue(() => TestJob(expectedCpuUsage));
            var finishStatus = hangfire.WaitForJobsCount(1);
            Assert.AreEqual(true, finishStatus.Success);
            Assert.IsTrue(isNotified);
            Assert.GreaterOrEqual(actualCpuUsage, expectedCpuUsage);
            Assert.LessOrEqual(actualCpuUsage, expectedCpuUsage * 2);

        }

        public void TestJob(int cpuLoad)
        {
            Console.WriteLine("Hello, it is a job");
            CpuLoader.Run(1, cpuLoad);
        }
    }
}