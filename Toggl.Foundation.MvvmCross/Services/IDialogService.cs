using System.Threading.Tasks;

namespace Toggl.Foundation.MvvmCross.Services
{
    public interface IDialogService
    {
        Task<bool> Confirm(
            string title,
            string message,
            string confirmButtonText,
            string dismissButtonText);

        Task<string> ShowMultipleChoiceDialog(
            string cancelText,
            params MultipleChoiceDialogAction[] actions);

        Task<object> Alert(string title, string message, string buttonTitle);
    }
}
