using System.Threading.Tasks;
using TermoHub.Models;

namespace TermoHub.Services
{
    public interface IAlertReporter
    {
        Task Report(Reading reading);
    }
}
