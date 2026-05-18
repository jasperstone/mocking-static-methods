using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache
{
    public class CacheDecoratorTests
    {
        [Fact]
        public void TryExtractKeyframes_KeyframeDataExists_LogsDebugMessage()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "test.mp4";
            var keyframeData = new KeyframeData(100, new List<long> { 1, 2, 3 });

            var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            keyframeExtractorMock.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out keyframeData)).Returns(true);

            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(null, keyframeExtractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            Assert.True(result);
            Assert.Equal(keyframeData, extractedKeyframeData);
            keyframeExtractorMock.Verify(extractor => extractor.TryExtractKeyframes(itemId, filePath, out extractedKeyframeData), Times.Never);
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully extracted keyframes using")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public void TryExtractKeyframes_KeyframeDataDoesNotExist_LogsDebugMessage()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "test.mp4";
            KeyframeData? keyframeData = null;

            var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            keyframeExtractorMock.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out keyframeData)).Returns(true);

            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(null, keyframeExtractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            Assert.True(result);
            Assert.Equal(keyframeData, extractedKeyframeData);
            keyframeExtractorMock.Verify(extractor => extractor.TryExtractKeyframes(itemId, filePath, out extractedKeyframeData), Times.Once);
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully extracted keyframes using")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public void TryExtractKeyframes_KeyframeDataDoesNotExistAndExtractionFails_LogsDebugMessage()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "test.mp4";
            KeyframeData? keyframeData = null;

            var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            keyframeExtractorMock.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out keyframeData)).Returns(false);

            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(null, keyframeExtractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            Assert.False(result);
            Assert.Null(extractedKeyframeData);
            keyframeExtractorMock.Verify(extractor => extractor.TryExtractKeyframes(itemId, filePath, out extractedKeyframeData), Times.Once);
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to extract keyframes using")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
