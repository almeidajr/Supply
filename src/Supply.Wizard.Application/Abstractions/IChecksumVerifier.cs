namespace Supply.Wizard.Application.Abstractions;

/// <summary>
/// Verifies artifact integrity.
/// </summary>
public interface IChecksumVerifier
{
    /// <summary>
    /// Verifies that a file matches the expected SHA-256 checksum.
    /// </summary>
    /// <param name="filePath">Absolute path to the file to verify.</param>
    /// <param name="expectedSha256">Expected SHA-256 hash value in hexadecimal format.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that completes when verification succeeds.</returns>
    Task VerifySha256Async(string filePath, string expectedSha256, CancellationToken cancellationToken);
}
