using System;
using System.Collections.Generic;
using System.Threading;
using Jellyfin.MediaEncoding.Hls.Cache;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache
{
    public class CacheDecoratorTests
    {
        private readonly Mock<IKeyframeRepository> _mockRepository;
        private readonly Mock<IKeyframeExtractor> _mockExtractor;
        private readonly Mock<ILogger<CacheDecorator>> _mockLogger;
        private readonly CacheDecorator _cacheDecorator;
        private readonly string _extractorName = "MockExtractor";

        public CacheDecoratorTests()
        {
            _mockRepository = new Mock<IKeyframeRepository>();
            _mockExtractor = new Mock<IKeyframeExtractor>();
            _mockLogger = new Mock<ILogger<CacheDecorator>>();

            _mockExtractor.Setup(e => e.GetType().Name).Returns(_extractorName);
            _mockExtractor.Setup(e => e.IsMetadataBased).Returns(true);

            _cacheDecorator = new CacheDecorator(_mockRepository.Object, _mockExtractor.Object, _mockLogger.Object);
        }

        [Fact]
        public void TryExtractKeyframes_ShouldLogDebug_WhenExtractionFails()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "path/to/file";
            KeyframeData outData;

            _mockRepository.Setup(r => r.GetKeyframeData(itemId))
                .Returns(new List<KeyframeData>());

            _mockExtractor.Setup(e => e.TryExtractKeyframes(itemId, filePath, out outData))
                .Returns(false);

            // Act
            var result = _cacheDecorator.TryExtractKeyframes(itemId, filePath, out var keyframeData);

            // Assert
            Assert.False(result);
            Assert.Null(keyframeData);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to extract keyframes using")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
