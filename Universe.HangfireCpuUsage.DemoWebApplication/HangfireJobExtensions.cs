using System.Reflection;
using System.Text;
using System.Text.Json;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;

namespace Universe.HangfireCpuUsage.DemoWebApplication
{
    public static class HangfireJobExtensions
    {
        public static string FormatArguments(this Job job)
        {
            StringBuilder ret = new StringBuilder($"{job.Method.Name}(");
            ParameterInfo[] parameters = job.Method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0) ret.Append(", ");
                ret.Append($"{parameters[i].Name} = ");
                string val = job.Args.Count > i ? JsonSerializer.Serialize(job.Args[i]) : "?";
                ret.Append(val);
            }

            return ret.Append(")").ToString();
        }
    }

    public static class HangfireCpuUsageConfigurationExtensions
    {
        public static IGlobalConfiguration AddCpuUsageHandler(this IGlobalConfiguration configuration, Action<PerformedContext, JobCpuUsage> handler)
        {
            var filter = GlobalJobFilters.Filters.Select(x => x.Instance as CpuUsageJobFilter).FirstOrDefault(x => x != null);
            if (filter == null)
            {
                filter = new CpuUsageJobFilter(delegate { });
                configuration.UseFilter(filter);
            }
            filter.AddHandler(handler);
            return configuration;
        }
    }
}
