using LiteNetwork.Server;
using Marionette.Shared;

namespace Marionette.Networking.Local;

internal class LocalMarionetteServer : LiteServer<LocalMarionetteUser>
{
    public LocalMarionetteServer(LiteServerOptions options, IServiceProvider? serviceProvider = null) : base(options, serviceProvider)
    {

    }
}