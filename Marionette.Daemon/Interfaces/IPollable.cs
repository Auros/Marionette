namespace Marionette.Daemon.Interfaces;

internal interface IPollable
{
    void Poll(CancellationToken cancellationToken);
}