using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Cache.Tests
{
    public class CacheDecoratorTests
    {
        [Fact]
        public void TryExtractKeyframes_ShouldLogDebug_WhenKeyframeExtractionFails()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "test.mp4";
            var keyframeExtractorName = "TestExtractor";

            var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
            keyframeRepositoryMock.Setup(repo => repo.GetKeyframeData(itemId)).Returns(new List<KeyframeData>().AsReadOnly());

            var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            keyframeExtractorMock.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData?>.IsAny)).Returns(false);
            keyframeExtractorMock.Setup(extractor => extractor.GetType().Name).Returns(keyframeExtractorName);

            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var keyframeData);

            // Assert
            keyframeExtractorMock.Verify(extractor => extractor.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData?>.IsAny), Times.Once);
            loggerMock.Verify(logger => logger.LogDebug("Failed to extract keyframes using {ExtractorName}", keyframeExtractorName), Times.Once);
            Assert.False(result);
            Assert.Null(keyframeData);
        }

        [Fact]
        public void TryExtractKeyframes_ShouldLogDebug_WhenKeyframeExtractionSucceeds()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "test.mp4";
            var keyframeExtractorName = "TestExtractor";
            var keyframeData = new KeyframeData(100, new long[] { 1, 2, 3 });

            var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
            keyframeRepositoryMock.Setup(repo => repo.GetKeyframeData(itemId)).Returns(new List<KeyframeData>().AsReadOnly());

            var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            keyframeExtractorMock.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData?>.IsAny))
                .Returns((Guid id, string path, out KeyframeData? data) =>
                {
                    data = keyframeData;
                    return true;
                });
            keyframeExtractorMock.Setup(extractor => extractor.GetType().Name).Returns(keyframeExtractorName);

            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            keyframeExtractorMock.Verify(extractor => extractor.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData?>.IsAny), Times.Once);
            loggerMock.Verify(logger => logger.LogDebug("Successfully extracted keyframes using {ExtractorName}", keyframeExtractorName), Times.Once);
            Assert.True(result);
            Assert.Equal(keyframeData, extractedKeyframeData);
        }
    }
}
