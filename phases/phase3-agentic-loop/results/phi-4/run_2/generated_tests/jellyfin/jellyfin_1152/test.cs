using System;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class CacheDecoratorTests
{
    [Fact]
    public void TryExtractKeyframes_LogsFailureWhenExtractionFails()
    {
        // Arrange
        var mockKeyframeRepository = new Mock<IKeyframeRepository>();
        var mockKeyframeExtractor = new Mock<IKeyframeExtractor>();
        var mockLogger = new Mock<ILogger<CacheDecorator>>();

        var itemId = Guid.NewGuid();
        var filePath = "test.mp4";
        KeyframeData keyframeData;

        mockKeyframeExtractor
            .Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out keyframeData))
            .Returns(false);

        var cacheDecorator = new CacheDecorator(mockKeyframeRepository.Object, mockKeyframeExtractor.Object, mockLogger.Object);

        // Act
        bool result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out _);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            logger => logger.LogDebug(
                "Failed to extract keyframes using {ExtractorName}",
                It.IsAny<string>()
            ),
            Times.Once
        );
    }
}
