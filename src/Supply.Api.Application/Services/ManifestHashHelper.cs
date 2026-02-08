using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Supply.Api.Application.Services;

public static class ManifestHashHelper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static string ComputeStrongETag<T>(T model)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(model, SerializerOptions);
        var hash = SHA256.HashData(payload);
        var hex = Convert.ToHexString(hash).ToLowerInvariant();
        return $"\"sha256:{hex}\"";
    }

    public static string ComputeSha256(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
