using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache;

public class CacheDecoratorTests
{
    private readonly Mock<IKeyframeRepository> _keyframeRepositoryMock;
    private readonly Mock<IKeyframeExtractor> _keyframeExtractorMock;
    private readonly Mock<ILogger<CacheDecorator>> _loggerMock;

    public CacheDecoratorTests()
    {
        _keyframeRepositoryMock = new Mock<IKeyframeRepository>();
        _keyframeExtractorMock = new Mock<IKeyframeExtractor>();
        _loggerMock = new Mock<ILogger<CacheDecorator>>();
    }

    [Fact]
    public void TryExtractKeyframes_KeyframeDataExists_ReturnsTrue()
    {
        // Arrange
        var cacheDecorator = new CacheDecorator(_keyframeRepositoryMock.Object, _keyframeExtractorMock.Object, _loggerMock.Object);
        var keyframeData = new KeyframeData();
        _keyframeRepositoryMock.Setup(k => k.GetKeyframeData(It.IsAny<Guid>())).Returns(new[] { keyframeData });

        // Act
        var result = cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), string.Empty, out var extractedKeyframeData);

        // Assert
        Assert.True(result);
        Assert.Same(keyframeData, extractedKeyframeData);
    }

    [Fact]
    public void TryExtractKeyframes_KeyframeDataDoesNotExist_KeyframeExtractorSucceeds_ReturnsTrue()
    {
        // Arrange
        var cacheDecorator = new CacheDecorator(_keyframeRepositoryMock.Object, _keyframeExtractorMock.Object, _loggerMock.Object);
        var keyframeData = new KeyframeData();
        _keyframeRepositoryMock.Setup(k => k.GetKeyframeData(It.IsAny<Guid>())).Returns(Enumerable.Empty<KeyframeData>());
        _keyframeExtractorMock.Setup(k => k.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out keyframeData)).Returns(true);

        // Act
        var result = cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), string.Empty, out var extractedKeyframeData);

        // Assert
        Assert.True(result);
        Assert.Same(keyframeData, extractedKeyframeData);
        _loggerMock.Verify(l => l.LogDebug("Successfully extracted keyframes using {ExtractorName}", _keyframeExtractorMock.Object.GetType().Name), Times.Once);
    }

    [Fact]
    public void TryExtractKeyframes_KeyframeDataDoesNotExist_KeyframeExtractorFails_ReturnsFalse()
    {
        // Arrange
        var cacheDecorator = new CacheDecorator(_keyframeRepositoryMock.Object, _keyframeExtractorMock.Object, _loggerMock.Object);
        _keyframeRepositoryMock.Setup(k => k.GetKeyframeData(It.IsAny<Guid>())).Returns(Enumerable.Empty<KeyframeData>());
        _keyframeExtractorMock.Setup(k => k.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData>.IsAny)).Returns(false);

        // Act
        var result = cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), string.Empty, out var extractedKeyframeData);

        // Assert
        Assert.False(result);
        Assert.Null(extractedKeyframeData);
        _loggerMock.Verify(l => l.LogDebug("Failed to extract keyframes using {ExtractorName}", _keyframeExtractorMock.Object.GetType().Name), Times.Once);
    }
}
