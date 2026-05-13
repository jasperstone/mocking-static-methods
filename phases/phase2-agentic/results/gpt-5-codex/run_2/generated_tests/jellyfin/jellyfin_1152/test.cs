using System;
using System.Threading;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache;

public class CacheDecoratorTests
{
    private readonly Guid _itemId = Guid.NewGuid();
    private const string FilePath = "/path/file.mkv";

    private CacheDecorator CreateDecorator(
        out Mock<IKeyframeRepository> repoMock,
        out Mock<IKeyframeExtractor> extractorMock,
        out Mock<ILogger<CacheDecorator>> loggerMock)
    {
        repoMock = new Mock<IKeyframeRepository>(MockBehavior.Strict);
        extractorMock = new Mock<IKeyframeExtractor>(MockBehavior.Strict);
        loggerMock = new Mock<ILogger<CacheDecorator>>();

        return new CacheDecorator(repoMock.Object, extractorMock.Object, loggerMock.Object);
    }

    [Fact]
    public void TryExtractKeyframes_WhenExtractorFails_LogsFailureAndReturnsFalse()
    {
        var decorator = CreateDecorator(out var repoMock, out var extractorMock, out var loggerMock);
        var keyframeData = new KeyframeData(Array.Empty<float>());

        repoMock.Setup(r => r.GetKeyframeData(_itemId)).Returns(new KeyframeData[] { });
        extractorMock.Setup(e => e.TryExtractKeyframes(_itemId, FilePath, out keyframeData))
            .Returns(false);
        extractorMock.SetupGet(e => e.IsMetadataBased).Throws(new InvalidOperationException());

        using var scope = new LogCaptureScope();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        var result = decorator.TryExtractKeyframes(_itemId, FilePath, out var _);

        Assert.False(result);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Failed to extract keyframes using {ExtractorName}"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        extractorMock.VerifyAll();
        repoMock.VerifyAll();
    }

    [Fact]
    public void TryExtractKeyframes_WhenExtractorSucceeds_LogsSuccessAndSaves()
    {
        var decorator = CreateDecorator(out var repoMock, out var extractorMock, out var loggerMock);
        var extracted = new KeyframeData(new[] { 1.0f, 2.0f });

        repoMock.Setup(r => r.GetKeyframeData(_itemId)).Returns(Array.Empty<KeyframeData>());
        extractorMock.Setup(e => e.TryExtractKeyframes(_itemId, FilePath, out extracted))
            .Returns(true);
        extractorMock.SetupGet(e => e.IsMetadataBased).Returns(false);
        repoMock.Setup(r => r.SaveKeyframeDataAsync(_itemId, extracted, CancellationToken.None))
            .ReturnsAsync(true);

        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        var result = decorator.TryExtractKeyframes(_itemId, FilePath, out var cached);

        Assert.True(result);
        Assert.Same(extracted, cached);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Successfully extracted keyframes using {ExtractorName}"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        repoMock.Verify(r => r.SaveKeyframeDataAsync(_itemId, extracted, CancellationToken.None), Times.Once);
        extractorMock.VerifyAll();
        repoMock.VerifyAll();
    }
}
