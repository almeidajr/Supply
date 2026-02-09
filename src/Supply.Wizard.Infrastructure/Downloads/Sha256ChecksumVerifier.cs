using System.Security.Cryptography;
using Supply.Wizard.Application.Abstractions;
using Supply.Wizard.Application.Exceptions;

namespace Supply.Wizard.Infrastructure.Downloads;

/// <summary>
/// SHA-256 checksum verifier.
/// </summary>
public sealed class Sha256ChecksumVerifier : IChecksumVerifier
{
    /// <summary>
    /// Verifies that the file content hash matches the expected SHA-256 value.
    /// </summary>
    /// <param name="filePath">Absolute path to the downloaded artifact file.</param>
    /// <param name="expectedSha256">Expected SHA-256 digest in hexadecimal format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task VerifySha256Async(string filePath, string expectedSha256, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            throw new ArtifactIntegrityException($"Artifact file '{filePath}' does not exist.");
        }

        var normalizedExpected = NormalizeHex(expectedSha256);

        await using var stream = File.OpenRead(filePath);
        var hashBytes = await SHA256.HashDataAsync(stream, cancellationToken);
        var actual = Convert.ToHexString(hashBytes).ToLowerInvariant();

        if (!string.Equals(actual, normalizedExpected, StringComparison.Ordinal))
        {
            throw new ArtifactIntegrityException(
                $"Checksum verification failed for '{filePath}'. Expected '{normalizedExpected}', actual '{actual}'."
            );
        }
    }

    private static string NormalizeHex(string hex) =>
        hex.Replace("-", string.Empty, StringComparison.Ordinal).Trim().ToLowerInvariant();
}
