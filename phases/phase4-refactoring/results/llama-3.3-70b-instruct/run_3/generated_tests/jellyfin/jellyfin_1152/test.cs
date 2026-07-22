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
    public void TryExtractKeyframes_KeyframeDataExists_ReturnsTrue()
    {
        // Arrange
        var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
        var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
        var loggerMock = new Mock<ILogger<CacheDecorator>>();

        keyframeRepositoryMock.Setup(r => r.GetKeyframeData(It.IsAny<Guid>()))
            .Returns(new List<KeyframeData> { new KeyframeData(100, new List<long>()) });

        var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

        // Act
        var result = cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), string.Empty, out _);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryExtractKeyframes_KeyframeDataDoesNotExist_KeyframeExtractorSucceeds_ReturnsTrue()
    {
        // Arrange
        var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
        var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
        var loggerMock = new Mock<ILogger<CacheDecorator>>();

        keyframeRepositoryMock.Setup(r => r.GetKeyframeData(It.IsAny<Guid>()))
            .Returns(new List<KeyframeData>());

        keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData>.IsAny))
            .Returns(true);

        var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

        // Act
        var result = cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), string.Empty, out _);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryExtractKeyframes_KeyframeDataDoesNotExist_KeyframeExtractorFails_ReturnsFalse()
    {
        // Arrange
        var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
        var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
        var loggerMock = new Mock<ILogger<CacheDecorator>>();

        keyframeRepositoryMock.Setup(r => r.GetKeyframeData(It.IsAny<Guid>()))
            .Returns(new List<KeyframeData>());

        keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData>.IsAny))
            .Returns(false);

        var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

        // Act
        var result = cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), string.Empty, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryExtractKeyframes_KeyframeDataDoesNotExist_KeyframeExtractorSucceeds_LogsDebugMessage()
    {
        // Arrange
        var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
        var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
        var loggerMock = new Mock<ILogger<CacheDecorator>>();

        keyframeRepositoryMock.Setup(r => r.GetKeyframeData(It.IsAny<Guid>()))
            .Returns(new List<KeyframeData>());

        keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData>.IsAny))
            .Returns(true);

        var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

        // Act
        cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), string.Empty, out _);

        // Assert
        loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void TryExtractKeyframes_KeyframeDataDoesNotExist_KeyframeExtractorFails_LogsDebugMessage()
    {
        // Arrange
        var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
        var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
        var loggerMock = new Mock<ILogger<CacheDecorator>>();

        keyframeRepositoryMock.Setup(r => r.GetKeyframeData(It.IsAny<Guid>()))
            .Returns(new List<KeyframeData>());

        keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData>.IsAny))
            .Returns(false);

        var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

        // Act
        cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), string.Empty, out _);

        // Assert
        loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }
}
