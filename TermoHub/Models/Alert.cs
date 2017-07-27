namespace TermoHub.Models
{
    public class Alert
    {
        public int AlertId { get; set; }
        /// <summary>
        /// The expected result from CompareTo with Limit
        /// </summary>
        public int Sign { get; set; }
        /// <summary>
        /// The min or max threshold depending on Sign
        /// </summary>
        public double Limit { get; set; }
        public string Email { get; set; }

        public Sensor Sensor { get; set; }

        public bool Check(double value) => value.CompareTo(Limit) == Sign;
    }
}
