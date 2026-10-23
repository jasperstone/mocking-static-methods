using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Keyframes;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class CacheDecoratorTests
{
    [Fact]
    public void TryExtractKeyframes_LogsDebugOnFailure()
    {
        // Arrange
        var mockKeyframeRepository = new Mock<IKeyframeRepository>();
        mockKeyframeRepository
            .Setup(repo => repo.GetKeyframeData(It.IsAny<Guid>()))
            .Returns(new List<KeyframeData>());

        var mockKeyframeExtractor = new Mock<IKeyframeExtractor>();
        mockKeyframeExtractor
            .Setup(extractor => extractor.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out KeyframeData result))
            .Returns(false);

        var mockLogger = new Mock<ILogger<CacheDecorator>>();

        var cacheDecorator = new CacheDecorator(
            mockKeyframeRepository.Object,
            mockKeyframeExtractor.Object,
            mockLogger.Object);

        // Act
        bool result = cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), "dummyFilePath", out KeyframeData _);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            logger => logger.LogDebug(
                "Failed to extract keyframes using {ExtractorName}",
                It.Is<object[]>(args => args[0].ToString() == mockKeyframeExtractor.Object.GetType().Name)),
            Times.Once);
    }
}
