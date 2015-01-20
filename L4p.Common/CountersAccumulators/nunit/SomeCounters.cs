namespace L4p.Common.CountersAccumulators.nunit
{
    class SomeCounters
    {
        public int One { get; set; }
        public int Two { get; set; }
        public double Three { get; set; }

        public InnerCounters Inner { get; set; }

        public class InnerCounters
        {
            public int InnerOne { get; set; }
            public double InnerDouble { get; set; }
        }
    }

    class OtherCounters
    {
        public int One { get; set; }
    }
}