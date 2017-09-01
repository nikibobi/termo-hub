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
            Sensor sensor = reading.Sensor;
            await context.Entry(sensor).Reference(s => s.Alert).LoadAsync();
            Alert alert = sensor.Alert;
            if (alert != null)
            {
                bool isAlert = alert.Check(reading.Value);
                if (isAlert != alert.IsNotified)
                {
                    alert.IsNotified = !alert.IsNotified;
                    await context.SaveChangesAsync();
                    string subject = FormatSubject(isAlert);
                    string message = FormatMessage(sensor, alert, reading);
                    await emailSender.SendEmailAsync(alert.Email, subject, message);
                }
            }
        }

        private string FormatSubject(bool isOn)
        {
            string prefix = isOn ? options.OnPrefix : options.OffPrefix;
            return $"{prefix} {options.Subject}";
        }

        private string FormatMessage(Sensor sensor, Alert alert, Reading reading)
        {
            DateTime time = reading.Time.ToLocalTime();
            string query = $"from={time.ToUtcString()}";
            string url = $"{options.Hostname}/{sensor.DeviceId}/{sensor.SensorId}?{query}";
            return $@"<a href=""{url}"">{sensor.NameOrId()}</a> reached limit of {alert.Limit} with {reading.Value} on {time}";
        }
    }
}
