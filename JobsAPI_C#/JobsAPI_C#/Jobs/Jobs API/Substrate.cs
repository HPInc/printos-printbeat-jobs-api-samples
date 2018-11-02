namespace Jobs
{
    class Substrate
    {
        public SubstrateCount[] counts { get; set; }
    }

    class SubstrateCount
    {
        public string name { get; set; }
        public int amountUsed { get; set; }
    }
}
