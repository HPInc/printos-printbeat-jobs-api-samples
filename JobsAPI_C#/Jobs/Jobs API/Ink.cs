namespace Jobs
{ 
    class Ink
    {
        public InkCount[] counts { get; set; }
    }

    class InkCount
    {
        public string name { get; set; }
        public int amountUsed { get; set; }
    }
}
