using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache;

public class CacheDecoratorTests
{
    [Fact]
    public void TryExtractKeyframes_LogsDebugWhenKeyframeExtractorFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CacheDecorator>>();
        var keyframeRepositoryMock = new Mock<Jellyfin.MediaEncoding.Hls.Cache.IKeyframeRepository>();
        var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
        keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData?>.IsAny))
            .Returns(false);
        var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

        // Act
        cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), "testFilePath", out _);

        // Assert
        loggerMock.Verify(l => l.LogDebug("Failed to extract keyframes using {ExtractorName}", keyframeExtractorMock.Object.GetType().Name), Times.Once);
    }

    [Fact]
    public void TryExtractKeyframes_LogsDebugWhenKeyframeExtractorSucceeds()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CacheDecorator>>();
        var keyframeRepositoryMock = new Mock<Jellyfin.MediaEncoding.Hls.Cache.IKeyframeRepository>();
        var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
        keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData?>.IsAny))
            .Returns(true);
        var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

        // Act
        cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), "testFilePath", out _);

        // Assert
        loggerMock.Verify(l => l.LogDebug("Successfully extracted keyframes using {ExtractorName}", keyframeExtractorMock.Object.GetType().Name), Times.Once);
    }
}
