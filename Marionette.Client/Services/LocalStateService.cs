using System;

namespace Marionette.Services;

public class LocalStateService
{
    public event Action<bool>? PingCycle;

    public void UpdatePing(bool value)
    {
        PingCycle?.Invoke(value);
    }
}