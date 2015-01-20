namespace L4p.Common.ForeverThreads
{
    public class ForeverThreadConfig
    {
        public string Name { get; set; }
        public int StartTimeout = 2000;
        public int StopTimeout = 1000;
    }
}