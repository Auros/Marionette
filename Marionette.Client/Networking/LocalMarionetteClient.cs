using LiteNetwork.Client;
using System;

namespace Marionette.Networking;

internal class LocalMarionetteClient : LiteClient
{
    public LocalMarionetteClient(LiteClientOptions options, IServiceProvider? serviceProvider = null) : base(options, serviceProvider)
    {
    }
}