using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public void TryExtractKeyframes_KeyframeDataExists_ReturnsTrue()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "test.mp4";
            var keyframeData = new KeyframeData(100, new List<long> { 1, 2, 3 });

            var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
            keyframeRepositoryMock.Setup(repo => repo.GetKeyframeData(itemId)).Returns(new List<KeyframeData> { keyframeData });

            var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            Assert.True(result);
            Assert.Equal(keyframeData, extractedKeyframeData);
            keyframeRepositoryMock.Verify(repo => repo.GetKeyframeData(itemId), Times.Once);
            keyframeExtractorMock.Verify(extractor => extractor.TryExtractKeyframes(itemId, filePath, out extractedKeyframeData), Times.Never);
            loggerMock.Verify(logger => logger.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void TryExtractKeyframes_KeyframeDataDoesNotExistAndExtractionFails_ReturnsFalseAndLogsFailure()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "test.mp4";

            var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
            keyframeRepositoryMock.Setup(repo => repo.GetKeyframeData(itemId)).Returns(Enumerable.Empty<KeyframeData>());

            var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            keyframeExtractorMock.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData?>.IsAny)).Returns(false);

            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            Assert.False(result);
            Assert.Null(extractedKeyframeData);
            keyframeRepositoryMock.Verify(repo => repo.GetKeyframeData(itemId), Times.Once);
            keyframeExtractorMock.Verify(extractor => extractor.TryExtractKeyframes(itemId, filePath, out extractedKeyframeData), Times.Once);
            loggerMock.Verify(logger => logger.LogDebug("Failed to extract keyframes using {ExtractorName}", It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryExtractKeyframes_KeyframeDataDoesNotExistAndExtractionSucceeds_ReturnsTrueAndLogsSuccess()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "test.mp4";
            var keyframeData = new KeyframeData(100, new List<long> { 1, 2, 3 });

            var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
            keyframeRepositoryMock.Setup(repo => repo.GetKeyframeData(itemId)).Returns(Enumerable.Empty<KeyframeData>());

            var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            keyframeExtractorMock.Setup(extractor => extractor.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData?>.IsAny))
                .Callback((Guid _, string _, out KeyframeData? data) => data = keyframeData)
                .Returns(true);

            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(keyframeRepositoryMock.Object, keyframeExtractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            Assert.True(result);
            Assert.Equal(keyframeData, extractedKeyframeData);
            keyframeRepositoryMock.Verify(repo => repo.GetKeyframeData(itemId), Times.Once);
            keyframeExtractorMock.Verify(extractor => extractor.TryExtractKeyframes(itemId, filePath, out extractedKeyframeData), Times.Once);
            loggerMock.Verify(logger => logger.LogDebug("Successfully extracted keyframes using {ExtractorName}", It.IsAny<object[]>()), Times.Once);
            keyframeRepositoryMock.Verify(repo => repo.SaveKeyframeDataAsync(itemId, keyframeData, CancellationToken.None), Times.Once);
        }
    }
}
