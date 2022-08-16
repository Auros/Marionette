namespace Marionette.Daemon.Services;

internal class LocalStateService
{
    public event Action? WantsToUpdate;

    public void Update()
    {
        WantsToUpdate?.Invoke();
    }
}