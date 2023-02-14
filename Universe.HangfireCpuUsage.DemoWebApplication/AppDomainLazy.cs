using Hangfire.Storage.Monitoring;

namespace Universe.HangfireCpuUsage.DemoWebApplication
{
    public class AppDomainLazy
    {
        public static T Get<T>(string key, Func<string, T> factory)
        {
            string name = key + ":" + typeof(T).FullName;
            var currentDomain = AppDomain.CurrentDomain;
            object raw = currentDomain.GetData(name);
            T ret;
            if (raw == null)
            {
                ret = factory(name);
                currentDomain.SetData(name, ret);
                return ret;
            }

            return (T)raw;
        }

        public static T Get<T>(Func<string, T> factory)
        {
            return Get(string.Empty, factory);
        }
    }
}
