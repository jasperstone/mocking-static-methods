using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache;

public class CacheDecoratorTests
{
    [Fact]
    public void TryExtractKeyframes_LogsDebugOnFailure()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CacheDecorator>>();
        var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
        var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
        keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData?>.IsAny))
            .Returns(false);
        var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

        // Act
        cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), "testFile", out _);

        // Assert
        loggerMock.Verify(l => l.LogDebug("Failed to extract keyframes using {ExtractorName}", keyframeExtractorMock.Object.GetType().Name), Times.Once);
    }

    [Fact]
    public void TryExtractKeyframes_LogsDebugOnSuccess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CacheDecorator>>();
        var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
        var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
        keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData?>.IsAny))
            .Returns(true);
        var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

        // Act
        cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), "testFile", out _);

        // Assert
        loggerMock.Verify(l => l.LogDebug("Successfully extracted keyframes using {ExtractorName}", keyframeExtractorMock.Object.GetType().Name), Times.Once);
    }
}
