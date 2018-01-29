using System.Threading.Tasks;

namespace Toggl.Foundation.Services
{
    public interface IMailService
    {
        Task<MailResult> Send(string recipient, string subject, string message);
    }

    public struct MailResult
    {
        public bool Success { get; }

        public string ErrorTitle { get; }

        public string ErrorMessage { get; }

        public MailResult(bool success, string errorTitle, string errorMessage)
        {
            Success = success;
            ErrorTitle = errorTitle;
            ErrorMessage = errorMessage;
        }
    }
}
