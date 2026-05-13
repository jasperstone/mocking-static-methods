using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Cache.Tests
{
    public class CacheDecoratorTests
    {
        [Fact]
        public void TryExtractKeyframes_LogsDebugMessageOnFailure()
        {
            // Arrange
            var mockKeyframeRepository = new Mock<IKeyframeRepository>();
            var mockKeyframeExtractor = new Mock<IKeyframeExtractor>();
            var mockLogger = new Mock<ILogger<CacheDecorator>>();

            mockKeyframeRepository.Setup(repo => repo.GetKeyframeData(It.IsAny<Guid>()))
                                  .Returns(Enumerable.Empty<KeyframeData>());

            mockKeyframeExtractor.Setup(extractor => extractor.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out KeyframeData _))
                                 .Returns(false);

            var cacheDecorator = new CacheDecorator(mockKeyframeRepository.Object, mockKeyframeExtractor.Object, mockLogger.Object);

            // Act
            bool result = cacheDecorator.TryExtractKeyframes(Guid.NewGuid(), "dummyFilePath");

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                logger => logger.LogDebug(
                    It.Is<string>(s => s.Contains("Failed to extract keyframes using")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
