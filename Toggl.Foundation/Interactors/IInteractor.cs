namespace Toggl.Foundation.Interactors
{
    public interface IInteractor<out T>
    {
        T Execute();
    }
}
