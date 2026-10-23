using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache
{
    public class CacheDecoratorTests
    {
        [Fact]
        public void TryExtractKeyframes_ShouldLogDebug_WhenKeyframeExtractionFails()
        {
            // Arrange
            var mockKeyframeRepository = new Mock<IKeyframeRepository>();
            var mockKeyframeExtractor = new Mock<IKeyframeExtractor>();
            var mockLogger = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(mockKeyframeRepository.Object, mockKeyframeExtractor.Object, mockLogger.Object);

            var itemId = Guid.NewGuid();
            var filePath = "testFilePath";

            mockKeyframeRepository.Setup(repo => repo.GetKeyframeData(itemId)).Returns(new List<KeyframeData>());
            mockKeyframeExtractor.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData?>.IsAny)).Returns(false);

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
        public void TryExtractKeyframes_ShouldLogDebug_WhenKeyframeExtractionSucceeds()
        {
            // Arrange
            var mockKeyframeRepository = new Mock<IKeyframeRepository>();
            var mockKeyframeExtractor = new Mock<IKeyframeExtractor>();
            var mockLogger = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(mockKeyframeRepository.Object, mockKeyframeExtractor.Object, mockLogger.Object);

            var itemId = Guid.NewGuid();
            var filePath = "testFilePath";
            var keyframeData = new KeyframeData(100, new List<long> { 1, 2, 3 });

            mockKeyframeRepository.Setup(repo => repo.GetKeyframeData(itemId)).Returns(new List<KeyframeData>());
            mockKeyframeExtractor.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData?>.IsAny))
                .Callback((Guid id, string path, out KeyframeData? data) => data = keyframeData)
                .Returns(true);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug("Successfully extracted keyframes using {ExtractorName}", It.IsAny<string>()),
                Times.Once);
            Assert.True(result);
            Assert.Equal(keyframeData, extractedKeyframeData);
        }
    }
}
