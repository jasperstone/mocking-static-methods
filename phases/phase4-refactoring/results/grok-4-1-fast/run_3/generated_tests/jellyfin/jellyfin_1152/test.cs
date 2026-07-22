using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache;

public class CacheDecoratorTests
{
    private readonly Mock<IKeyframeRepository> _mockRepository;
    private readonly Mock<IKeyframeExtractor> _mockExtractor;
    private readonly Mock<ILogger<CacheDecorator>> _mockLogger;
    private readonly CacheDecorator _cacheDecorator;

    public CacheDecoratorTests()
    {
        _mockRepository = new Mock<IKeyframeRepository>();
        _mockExtractor = new Mock<IKeyframeExtractor>();
        _mockLogger = new Mock<ILogger<CacheDecorator>>();

        _mockExtractor.Setup(e => e.IsMetadataBased).Returns(false);

        _cacheDecorator = new CacheDecorator(_mockRepository.Object, _mockExtractor.Object, _mockLogger.Object);
    }

    [Fact]
    public void TryExtractKeyframes_CacheHit_ReturnsTrue()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var cachedData = new KeyframeData(10000000, new List<long> { 0, 5000000 });
        _mockRepository.Setup(r => r.GetKeyframeData(itemId)).Returns(new[] { cachedData }.ToList().AsReadOnly());

        // Act
        var result = _cacheDecorator.TryExtractKeyframes(itemId, "test.mp4", out var keyframeData);

        // Assert
        Assert.True(result);
        Assert.Equal(cachedData, keyframeData);
        _mockLogger.VerifyNoOtherCalls();
    }

    [Fact]
    public void TryExtractKeyframes_CacheMiss_ExtractorFails_LogsFailureMessage()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var extractorName = _mockExtractor.Object.GetType().Name;
        _mockRepository.Setup(r => r.GetKeyframeData(itemId)).Returns(new List<KeyframeData>().AsReadOnly());
        _mockExtractor.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData?>.IsAny))
                     .Returns(false);

        // Act
        var result = _cacheDecorator.TryExtractKeyframes(itemId, "test.mp4", out var keyframeData);

        // Assert
        Assert.False(result);
        Assert.Null(keyframeData);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to extract keyframes using {extractorName}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void TryExtractKeyframes_CacheMiss_ExtractorSucceeds_LogsSuccessMessage()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var extractedData = new KeyframeData(10000000, new List<long> { 0, 5000000 });
        var extractorName = _mockExtractor.Object.GetType().Name;

        _mockRepository.Setup(r => r.GetKeyframeData(itemId)).Returns(new List<KeyframeData>().AsReadOnly());
        _mockExtractor.Setup(e => e.TryExtractKeyframes(itemId, "test.mp4", out var data))
                     .Returns(true)
                     .Callback<Guid, string, KeyframeData?>((id, path, data) => data = extractedData);

        // Act
        var result = _cacheDecorator.TryExtractKeyframes(itemId, "test.mp4", out var keyframeData);

        // Assert
        Assert.True(result);
        Assert.Equal(extractedData, keyframeData);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Successfully extracted keyframes using {extractorName}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _mockRepository.Verify(r => r.SaveKeyframeDataAsync(itemId, extractedData, It.IsAny<CancellationToken>()), Times.Once);
    }
}
