namespace Universe.HangfireCpuUsage
{
    public class JobCpuUsage
    {
        public double Duration { get; internal set; }
        public double KernelTime { get; internal set; }
        public double UserTime { get; internal set; }
        public int SubTaskCount { get; internal set; }
        public string InfoMessage { get; internal set; }
        public bool HasValue => InfoMessage != null;

        public override string ToString()
        {
            return InfoMessage;
        }
    }
}