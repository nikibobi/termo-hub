using System;
using System.Threading.Tasks;
using TermoHub.Extensions;
using TermoHub.Models;

namespace TermoHub.Services
{
    public class AlertReporter : IAlertReporter
    {
        private const string Subject = "Alert!";

        private readonly TermoHubContext context;
        private readonly IEmailSender emailSender;

        public AlertReporter(TermoHubContext context, IEmailSender emailSender)
        {
            this.context = context;
            this.emailSender = emailSender;
        }

        public async Task Report(Reading reading)
        {
            if (reading == null)
                throw new ArgumentNullException(nameof(reading));

            await context.Entry(reading).Reference(r => r.Sensor).LoadAsync();
            Sensor sensor = reading.Sensor;
            await context.Entry(sensor).Reference(s => s.Alert).LoadAsync();
            Alert alert = sensor.Alert;
            if (alert != null && alert.Check(reading.Value))
            {
                string message = $"{sensor.NameOrId()} reached limit {alert.Limit} with {reading.Value}";
                await emailSender.SendEmailAsync(alert.Email, Subject, message);
            }
        }
    }
}
