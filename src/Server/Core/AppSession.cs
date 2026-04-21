using LuciferCore.Attributes;
using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.NetCoreServer.Session;
using System.Runtime.CompilerServices;

namespace Server.Core;

[RateLimiter(10, 1)]
public class AppSession : WssSession
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AppSession(AppServer server) : base(server) { }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        base.OnReceived(buffer, offset, size);
    }

    /// <summary>Handles incoming WebSocket binary messages by dispatching them to the Lucifer dispatcher.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void OnWsReceived(byte[] buffer, long offset, long size)
    {
        Lucifer.Dispatch(this, buffer, offset, size, 0.001f);
    }

    /// <summary>Handles incoming HTTP requests by dispatching them to the Lucifer dispatcher.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void OnReceivedRequest(RequestModel request)
    {
        Lucifer.Dispatch(this, request, 0.1f);
    }
}
