using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Supply.Api.Application.Services;

/// <summary>
/// Computes stable hashes used for manifest and payload ETags.
/// </summary>
public static class ManifestHashHelper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Computes a strong ETag value from a serialized model payload.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="model">Model instance to hash.</param>
    /// <returns>Strong ETag string value.</returns>
    public static string ComputeStrongETag<T>(T model)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(model, SerializerOptions);
        var hash = SHA256.HashData(payload);
        var hex = Convert.ToHexString(hash).ToLowerInvariant();
        return $"\"sha256:{hex}\"";
    }

    /// <summary>
    /// Computes a lowercase SHA-256 hex string for the provided value.
    /// </summary>
    /// <param name="value">Input value.</param>
    /// <returns>SHA-256 hash in lowercase hexadecimal.</returns>
    public static string ComputeSha256(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
