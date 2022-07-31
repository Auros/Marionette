using Marionette.Daemon.Interfaces;
using Microsoft.Extensions.Logging;
using OscCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Marionette.Daemon.Networking.OSC;

internal class OSCReceiver : IOSCReceiver, IPollable, IDisposable
{
    private IPEndPoint _listenOn;
    private readonly ILogger _logger;
    private readonly UdpClient _udpClient;

    private readonly List<byte[]> _compiledAddresses = new();
    private readonly Dictionary<int, List<Action<OscMessage>>> _callbacksById = new();

    private bool _disposed;
    private CancellationToken? _pollingToken;

    public OSCReceiver(ILogger<OSCReceiver> logger)
    {
        _logger = logger;

        // TODO: Load port from a config.
        // Assign the UDP Client with the listening port
        _udpClient = new(9001);
        _listenOn = new IPEndPoint(IPAddress.Any, 9001);

        // We start the receiver thread here
        Task.Run(Process);
    }

    public void Poll(CancellationToken cancellationToken)
    {
        _pollingToken = cancellationToken;
    }

    private void Process()
    {
        while (!_disposed)
        {
            if (!_pollingToken.HasValue)
                continue;

            if (_pollingToken.Value.IsCancellationRequested)
                return;

            _pollingToken = null;
            try
            {
                var buffer = _udpClient.Receive(ref _listenOn);
                DataReceived(buffer);
            }
            catch (SocketException socketException)
            {
                /* If the socket was cancelled, we don't care. If the socket gets disconnected then we're either just gonna not process the packet, or we'll reconnect later (internal daemon restart). */
                if (socketException.SocketErrorCode is SocketError.Interrupted)
                    continue;

                _logger.LogError(socketException, "An error occured while trying to reecive data");
            }
        }
    }

    private void DataReceived(byte[] buffer)
    {
        _logger.LogInformation("Received Packet, Count = {PacketCount}", buffer.Length);
        _logger.LogInformation("Data: {Data}", Encoding.ASCII.GetString(buffer));

        OscMessage? message = null;

        for (int i = 0; i < _compiledAddresses.Count; i++)
        {
            var compiled = _compiledAddresses[i];
            if (!DoesBufferMatchCompiledAddress(buffer, compiled))
                continue;

            // We have an address match! Invoke all the callbacks
            
            // We check if 'message' is null. If another compiled address matched earlier in the loop, we don't want to rebuild the OscMessage again.
            if (message == null)
            {
                message = OscMessage.Read(buffer, 0, buffer.Length);
                if (message.IsEmpty)
                {
                    // If we cannot parse this message, it's probably malformed.
                    _logger.LogWarning("Malformed buffer! Could not parse OSC message");
                    return;
                }
            }
            _callbacksById[i].ForEach(callback => callback.Invoke(message));
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _udpClient.Dispose();
        _disposed = true;
    }

    public void Subscribe(string address, Action<OscMessage> callback)
    {
        if (string.IsNullOrWhiteSpace(address))
            return;

        var compiledAddress = CompileAddress(address);
        var id = GetCompiledAddressId(compiledAddress);
        if (id == -1)
        {
            // If the id doesn't exist, add it to the callback map and compiled address list.
            id = _compiledAddresses.Count;
            _compiledAddresses.Add(compiledAddress);
            _callbacksById.Add(id, new List<Action<OscMessage>>() { callback });
            return;
        }

        // Add the callback if necessary
        if (_callbacksById.TryGetValue(id, out var callbacks) && !callbacks.Contains(callback))
            _callbacksById[id].Add(callback);
    }

    public void Unsubscribe(string address, Action<OscMessage> callback)
    {
        if (string.IsNullOrWhiteSpace(address))
            return;

        var compiledAddress = CompileAddress(address);
        var id = GetCompiledAddressId(compiledAddress);

        if (id == -1)
            return;

        _callbacksById[id].Remove(callback);
    }

    private static byte[] CompileAddress(string address)
    {
        return Encoding.ASCII.GetBytes(address);
    }

    // This method checks to see if a buffer (presumably received from a UDP socket in the format of OSC) matches a compiled address
    // This was designed to not allocate at all. Alloc stupid
    private static bool DoesBufferMatchCompiledAddress(byte[] buffer, byte[] compiledAddress)
    {
        // For now, I'm only supporting the OSC '*' keyword, because that's what I need. If and when I decide to turn this OSC stuff into its own library, I'll eventually support them all.
        // 0123456789
        // /avatar/*/Color
        // /avatar/parameters/Color

        bool valid = true;
        int bufferHeader = 0;
        for (int i = 0; i < compiledAddress.Length; i++)
        {
            // If the current state is now invalid, we break out just in case.
            if (!valid)
                break;

            // If we have no buffer left to analyze, break out.
            if (bufferHeader >= buffer.Length)
                break;

            // Once we hit the address terminator, ',', we break out. The value of 'valid' determines whether or not this matched successfully.
            if (buffer[bufferHeader] == ',')
                break;

            if (compiledAddress[i] == '*')
            {
                // If there's a *, that means we should accept everything in this address block... probably.
                // So, we move forward in the buffer until we hit the terminator or a path separator.

                for (int c = bufferHeader; c < buffer.Length; c++)
                {
                    if (c >= buffer.Length || buffer[c] == ',' || buffer[c] == '/')
                    {
                        // We advanced too far (seeked), so we subtract one and break out.
                        bufferHeader--;
                        break;
                    }

                    // Advance the buffer until we hit a terminator
                    bufferHeader++;
                }
            }
            else if (compiledAddress[i] != buffer[bufferHeader])
            {
                // If it doesn't match a reserved character and it doesn't match the current header, this address does not match.
                valid = false;
                break;
            }
            bufferHeader++;
        }

        return valid;
    }

    // This checks to see a compiled address exists in the compiled addresses list, and if it does, return its index.
    // This map 
    private int GetCompiledAddressId(byte[] compiledAddress)
    {
        for (int i = 0; i < _compiledAddresses.Count; i++)
        {
            var compiled = _compiledAddresses[i];
            if (compiled.Length != compiledAddress.Length) // If the arrays aren't the same length, skip, since we know they can't be equal
                continue;

            bool localValid = true;
            for (int c = 0; c < compiled.Length; c++) // Check each individual byte
            {
                if (compiled[c] != compiledAddress[c]) // If there's a difference, break out of the loop and continue
                {
                    localValid = false;
                    break;
                }
            }

            if (!localValid)
                continue;

            // If the byte array comparison succeeds, return the index of the compiled address we're at.
            return i;
        }
        return -1;
    }
}