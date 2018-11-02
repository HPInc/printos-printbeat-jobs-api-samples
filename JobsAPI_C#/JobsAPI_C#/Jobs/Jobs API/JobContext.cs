namespace Jobs
{
    class JobContext
    {
        public string Value { get; set; }

        private JobContext(string value) { Value = value; }

        public static JobContext Job { get { return new JobContext("job"); } }
        public static JobContext DFE { get { return new JobContext("dfe"); } }
        public static JobContext Press { get { return new JobContext("press"); } }
        public static JobContext PrintRun { get { return new JobContext("printrun"); } }
        public static JobContext Historic { get { return new JobContext("historic"); } }
    }
}
