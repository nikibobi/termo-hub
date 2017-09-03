using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TermoHub.Extensions;
using TermoHub.Models;
using TermoHub.Options;

namespace TermoHub.Services
{
    public class AlertReporter : IAlertReporter
    {
        private readonly ReporterOptions options;
        private readonly TermoHubContext context;
        private readonly IEmailSender emailSender;

        public AlertReporter(IOptions<ReporterOptions> options, TermoHubContext context, IEmailSender emailSender)
        {
            this.options = options.Value;
            this.context = context;
            this.emailSender = emailSender;
        }

        public async Task Report(Reading reading)
        {
            if (reading == null)
                throw new ArgumentNullException(nameof(reading));

            await context.Entry(reading).Reference(r => r.Sensor).LoadAsync();
            await context.Entry(reading.Sensor).Reference(s => s.Alert).LoadAsync();
            Alert alert = reading.Sensor.Alert;
            if (alert == null)
                return;

            bool isAlert = alert.Check(reading.Value);
            if (isAlert != alert.IsNotified)
            {
                alert.IsNotified = !alert.IsNotified;
                await context.SaveChangesAsync();
                string subject = FormatSubject(isAlert);
                string message = FormatMessage(isAlert, reading);
                await emailSender.SendEmailAsync(alert.Email, subject, message);
            }
        }

        private string FormatSubject(bool isAlert)
        {
            string prefix = isAlert ? options.OnPrefix : options.OffPrefix;
            return $"{prefix} {options.Subject}";
        }

        private string FormatMessage(bool isAlert, Reading reading)
        {
            Sensor sensor = reading.Sensor;
            Alert alert = sensor.Alert;
            int minutes = options.MinutesMargin;
            DateTime time = reading.Time.ToLocalTime();
            string from = time.AddMinutes(-minutes).ToUtcString();
            string to = time.AddMinutes(minutes).ToUtcString();
            string query = $"from={from}&to={to}";
            string term = isAlert ? "reached" : "fixed";
            string url = $"{options.Hostname}/{sensor.DeviceId}/{sensor.SensorId}?{query}";
            return $@"<a href=""{url}"">{sensor.NameOrId()}</a> {term} limit of {alert.Limit} with {reading.Value} on {time}";
        }
    }
}
