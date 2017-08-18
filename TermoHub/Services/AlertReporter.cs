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
                if (alert.Check(reading.Value))
                {
                    if (!alert.IsNotified)
                    {
                        string message = FormatMessage(sensor, alert, reading);
                        await emailSender.SendEmailAsync(alert.Email, options.Subject, message);
                        alert.IsNotified = true;
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    if (alert.IsNotified)
                    {
                        alert.IsNotified = false;
                        await context.SaveChangesAsync();
                    }
                }
            }
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
