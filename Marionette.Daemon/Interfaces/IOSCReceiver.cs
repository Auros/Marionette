using OscCore;

namespace Marionette.Daemon.Interfaces;

internal interface IOSCReceiver
{
    void Subscribe(string address, Action<OscMessage> callback);
    void Unsubscribe(string address, Action<OscMessage> callback);
}