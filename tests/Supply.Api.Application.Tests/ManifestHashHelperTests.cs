using Supply.Api.Application.Services;

namespace Supply.Api.Application.Tests;

public sealed class ManifestHashHelperTests
{
    [Fact]
    public void ComputeStrongETag_WhenModelIsSame_ShouldReturnDeterministicTag()
    {
        var model = new { Name = "agent", Version = "1.0.0" };

        var first = ManifestHashHelper.ComputeStrongETag(model);
        var second = ManifestHashHelper.ComputeStrongETag(model);

        Assert.Equal(first, second);
        Assert.Matches("^\"sha256:[0-9a-f]{64}\"$", first);
    }

    [Fact]
    public void ComputeStrongETag_WhenModelChanges_ShouldReturnDifferentTag()
    {
        var first = ManifestHashHelper.ComputeStrongETag(new { Name = "agent", Version = "1.0.0" });
        var second = ManifestHashHelper.ComputeStrongETag(new { Name = "agent", Version = "2.0.0" });

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void ComputeSha256_WithKnownInput_ShouldMatchExpectedDigest()
    {
        var digest = ManifestHashHelper.ComputeSha256("abc");

        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", digest);
    }
}
