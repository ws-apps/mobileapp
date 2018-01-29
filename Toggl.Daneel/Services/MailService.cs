using System.Threading.Tasks;
using MessageUI;
using Toggl.Foundation;
using Toggl.Foundation.Services;
using Toggl.Multivac;

namespace Toggl.Daneel.Services
{
    public sealed class MailService : IMailService
    {
        private readonly ITopViewControllerProvider topViewControllerProvider;

        public MailService(ITopViewControllerProvider topViewControllerProvider)
        {
            Ensure.Argument.IsNotNull(topViewControllerProvider, nameof(topViewControllerProvider));

            this.topViewControllerProvider = topViewControllerProvider;
        }

        public Task<MailResult> Send(string recipient, string subject, string message)
        {
            var tcs = new TaskCompletionSource<MailResult>();

            if (!MFMailComposeViewController.CanSendMail)
            {
                var mailResult = new MailResult(
                    false,
                    Resources.NoEmailErrorTitle,
                    Resources.NoEmailErrorMessage);
                return Task.FromResult(mailResult);
            }

            var mailComposeViewController = new MFMailComposeViewController();
            mailComposeViewController.SetToRecipients(new[] { recipient });
            mailComposeViewController.SetSubject(subject);
            mailComposeViewController.SetMessageBody(message, false);

            mailComposeViewController.Finished += (sender, e) => {
                mailComposeViewController.DismissViewController(true, null);
                var mailResult = e.Result == MFMailComposeResult.Sent
                                  ? new MailResult(true, null, null)
                                  : new MailResult(false, null, null);
                tcs.SetResult(mailResult);
            };

            topViewControllerProvider.TopViewController.PresentViewController(mailComposeViewController, true, null);

            return tcs.Task;
        }
    }
}
