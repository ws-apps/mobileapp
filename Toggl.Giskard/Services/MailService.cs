using System;
using System.Threading.Tasks;
using Toggl.Foundation.Services;

namespace Toggl.Giskard.Services
{
    public sealed class MailService : IMailService
    {
        public Task<MailResult> Send(string recipient, string subject, string message)
        {
            throw new NotImplementedException();
        }
    }
}
