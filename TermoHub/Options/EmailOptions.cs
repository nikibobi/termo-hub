namespace TermoHub.Options
{
    public class EmailOptions
    {
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string EmailPassword { get; set; }
        public string SubjectPrefix { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
    }
}
