using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache
{
    public class CacheDecoratorTests
    {
        [Fact]
        public void TryExtractKeyframes_ShouldLogDebugWhenExtractionFails()
        {
            // Arrange
            var mockKeyframeRepository = new Mock<IKeyframeRepository>();
            var mockKeyframeExtractor = new Mock<IKeyframeExtractor>();
            var mockLogger = new Mock<ILogger<CacheDecorator>>();

            var itemId = Guid.NewGuid();
            var filePath = "testFilePath";

            mockKeyframeRepository.Setup(repo => repo.GetKeyframeData(itemId)).Returns(new List<KeyframeData>());
            mockKeyframeExtractor.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData?>.IsAny)).Returns(false);

            var cacheDecorator = new CacheDecorator(mockKeyframeRepository.Object, mockKeyframeExtractor.Object, mockLogger.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var keyframeData);

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug("Failed to extract keyframes using {ExtractorName}", It.IsAny<string>()),
                Times.Once);
            Assert.False(result);
            Assert.Null(keyframeData);
        }

        [Fact]
        public void TryExtractKeyframes_ShouldLogDebugWhenExtractionSucceeds()
        {
            // Arrange
            var mockKeyframeRepository = new Mock<IKeyframeRepository>();
            var mockKeyframeExtractor = new Mock<IKeyframeExtractor>();
            var mockLogger = new Mock<ILogger<CacheDecorator>>();

            var itemId = Guid.NewGuid();
            var filePath = "testFilePath";
            var keyframeData = new KeyframeData(1000, new List<long> { 100, 200, 300 });

            mockKeyframeRepository.Setup(repo => repo.GetKeyframeData(itemId)).Returns(new List<KeyframeData>());
            mockKeyframeExtractor.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData?>.IsAny))
                .Callback((Guid id, string path, out KeyframeData? data) => data = keyframeData)
                .Returns(true);

            var cacheDecorator = new CacheDecorator(mockKeyframeRepository.Object, mockKeyframeExtractor.Object, mockLogger.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug("Successfully extracted keyframes using {ExtractorName}", It.IsAny<string>()),
                Times.Once);
            Assert.True(result);
            Assert.Equal(keyframeData, extractedKeyframeData);
        }

        [Fact]
        public void TryExtractKeyframes_ShouldReturnTrueWhenKeyframeDataExists()
        {
            // Arrange
            var mockKeyframeRepository = new Mock<IKeyframeRepository>();
            var mockKeyframeExtractor = new Mock<IKeyframeExtractor>();
            var mockLogger = new Mock<ILogger<CacheDecorator>>();

            var itemId = Guid.NewGuid();
            var filePath = "testFilePath";
            var keyframeData = new KeyframeData(1000, new List<long> { 100, 200, 300 });

            mockKeyframeRepository.Setup(repo => repo.GetKeyframeData(itemId)).Returns(new List<KeyframeData> { keyframeData });

            var cacheDecorator = new CacheDecorator(mockKeyframeRepository.Object, mockKeyframeExtractor.Object, mockLogger.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            Assert.True(result);
            Assert.Equal(keyframeData, extractedKeyframeData);
        }
    }
}
