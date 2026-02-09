using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Supply.Wizard.Application.Exceptions;
using Supply.Wizard.Domain;

namespace Supply.Wizard.Infrastructure.Http;

internal static class HttpClientBuilder
{
    public static bool RequiresDedicatedTransport(WizardAuthOptions authentication, WizardTlsOptions tls) =>
        !string.IsNullOrWhiteSpace(authentication.ClientCertificateFilePath)
        || tls.AllowInsecureServerCertificate
        || !string.IsNullOrWhiteSpace(tls.CustomCaCertificateFilePath);

    public static async Task ApplyAuthenticationAsync(
        HttpRequestHeaders headers,
        WizardAuthOptions authentication,
        CancellationToken cancellationToken
    )
    {
        var bearerToken = await ReadBearerTokenAsync(authentication, cancellationToken);
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }

    public static async Task<HttpClient> CreateAsync(
        WizardAuthOptions authentication,
        WizardTlsOptions tls,
        CancellationToken cancellationToken
    )
    {
        var handler = CreateHandler(authentication, tls);
        var client = new HttpClient(handler, disposeHandler: true) { Timeout = TimeSpan.FromSeconds(60) };
        await ApplyAuthenticationAsync(client.DefaultRequestHeaders, authentication, cancellationToken);

        return client;
    }

    [SuppressMessage(
        "Security",
        "S4830:Server certificates should be verified during SSL/TLS connections",
        Justification = "Dev-only override behind explicit option."
    )]
    private static HttpClientHandler CreateHandler(WizardAuthOptions authentication, WizardTlsOptions tls)
    {
        var handler = new HttpClientHandler { SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13 };

        AttachClientCertificate(handler, authentication);

        if (tls.AllowInsecureServerCertificate)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            return handler;
        }

        if (!string.IsNullOrWhiteSpace(tls.CustomCaCertificateFilePath))
        {
            var customCa = LoadCertificate(tls.CustomCaCertificateFilePath);
            handler.ServerCertificateCustomValidationCallback = (_, certificate, _, _) =>
            {
                if (certificate is null)
                {
                    return false;
                }

                var serverCertificate = certificate;
                using X509Chain chain = new();
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.Add(customCa);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                return chain.Build(serverCertificate);
            };
        }

        return handler;
    }

    private static void AttachClientCertificate(HttpClientHandler handler, WizardAuthOptions authentication)
    {
        if (string.IsNullOrWhiteSpace(authentication.ClientCertificateFilePath))
        {
            return;
        }

        var certificate = string.IsNullOrWhiteSpace(authentication.ClientCertificateKeyFilePath)
            ? LoadCertificate(authentication.ClientCertificateFilePath)
            : X509Certificate2.CreateFromPemFile(
                authentication.ClientCertificateFilePath,
                authentication.ClientCertificateKeyFilePath
            );

        handler.ClientCertificates.Add(certificate);
    }

    private static X509Certificate2 LoadCertificate(string certificateFilePath)
    {
        var extension = Path.GetExtension(certificateFilePath);
        return extension.Equals(".pem", StringComparison.OrdinalIgnoreCase)
            ? X509Certificate2.CreateFromPemFile(certificateFilePath)
            : X509CertificateLoader.LoadCertificateFromFile(certificateFilePath);
    }

    private static async Task<string?> ReadBearerTokenAsync(
        WizardAuthOptions authentication,
        CancellationToken cancellationToken
    )
    {
        if (!string.IsNullOrWhiteSpace(authentication.BearerToken))
        {
            return authentication.BearerToken;
        }

        if (string.IsNullOrWhiteSpace(authentication.BearerTokenFilePath))
        {
            return null;
        }

        if (!File.Exists(authentication.BearerTokenFilePath))
        {
            throw new ApiAccessException($"Token file '{authentication.BearerTokenFilePath}' does not exist.");
        }

        var token = await File.ReadAllTextAsync(authentication.BearerTokenFilePath, cancellationToken);
        return string.IsNullOrWhiteSpace(token) ? null : token.Trim();
    }
}
