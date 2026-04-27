using LuciferCore.Attributes;
using LuciferCore.NetCoreServer.Server;
using LuciferCore.NetCoreServer.Transport.SSL;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Server.Core;

[Server("App Server", 8443)]
public class AppServer : WssServer
{
    /// <summary>Initializes a new instance of the WssServerBase class with specified SSL context, IP address, and port.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AppServer(SslContext context, IPAddress address, int port) : base(context, address, port)
    {
        AddStaticContent(_staticContentPath);
        Cache.Freeze();

        Mapping = new(true)
        {
            { "/","/index.html" },
            { "/404", "/404.html" },
        };
        Mapping.Freeze();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AppServer(int port) : this(CreateSslContext(), IPAddress.IPv6Any, port)
    {
        OptionDualMode = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override AppSession CreateSession() => new(this);

    [Config("WWWROOT", "assets/wwwroot")]
    private static string _staticContentPath { get; set; } = string.Empty;

    [Config("CERTIFICATE", "assets/tools/certificates/server.pfx")]
    private static string s_certPath { get; set; } = string.Empty;

    [Config("CERT_PASSWORD", "RootCA!SecureKey@Example2025Strong")]
    private static string s_certPassword { get; set; } = string.Empty;

    /// <summary>Creates an SSL context by loading a certificate from a specified path with a password.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SslContext CreateSslContext()
    {
#if DEBUG
        return SslContext.CreateDevelopmentContext();
#endif
        var cert = X509CertificateLoader.LoadPkcs12FromFile(s_certPath, s_certPassword);
        return new(SslProtocols.Tls12, cert);
    }
}
