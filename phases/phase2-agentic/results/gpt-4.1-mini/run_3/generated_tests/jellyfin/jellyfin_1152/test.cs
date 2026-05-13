using System;
using System.Collections.Generic;
using System.Threading;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Keyframes;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache
{
    public class CacheDecoratorTests
    {
        [Fact]
        public void TryExtractKeyframes_ReturnsTrue_WhenKeyframeDataExistsInRepository()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "somefile.mp4";
            var keyframeData = new KeyframeData(1000, new List<long> { 10, 20, 30 });

            var repoMock = new Mock<IKeyframeRepository>();
            repoMock.Setup(r => r.GetKeyframeData(itemId)).Returns(new[] { keyframeData });

            var extractorMock = new Mock<IKeyframeExtractor>();
            extractorMock.SetupGet(e => e.IsMetadataBased).Returns(false);

            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(repoMock.Object, extractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            Assert.True(result);
            Assert.Same(keyframeData, extractedKeyframeData);
            // No calls to extractor or logger expected
            extractorMock.Verify(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData>.IsAny), Times.Never);
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void TryExtractKeyframes_ReturnsFalse_AndLogsDebug_WhenExtractorFails()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "somefile.mp4";

            var repoMock = new Mock<IKeyframeRepository>();
            repoMock.Setup(r => r.GetKeyframeData(itemId)).Returns(Array.Empty<KeyframeData>());

            var extractorMock = new Mock<IKeyframeExtractor>();
            extractorMock.SetupGet(e => e.IsMetadataBased).Returns(false);
            KeyframeData? outData = null;
            extractorMock.Setup(e => e.TryExtractKeyframes(itemId, filePath, out outData)).Returns(false);

            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(repoMock.Object, extractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var extractedKeyframeData);

            // Assert
            Assert.False(result);
            Assert.Null(extractedKeyframeData);
            loggerMock.Verify(l => l.LogDebug("Failed to extract keyframes using {ExtractorName}", nameof(Mock<IKeyframeExtractor>)), Times.Once);
        }

        [Fact]
        public void TryExtractKeyframes_ReturnsTrue_AndSavesData_WhenExtractorSucceeds()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "somefile.mp4";
            var extractedData = new KeyframeData(2000, new List<long> { 5, 15, 25 });

            var repoMock = new Mock<IKeyframeRepository>();
            repoMock.Setup(r => r.GetKeyframeData(itemId)).Returns(Array.Empty<KeyframeData>());
            repoMock.Setup(r => r.SaveKeyframeDataAsync(itemId, extractedData, CancellationToken.None)).Returns(System.Threading.Tasks.Task.CompletedTask);

            var extractorMock = new Mock<IKeyframeExtractor>();
            extractorMock.SetupGet(e => e.IsMetadataBased).Returns(true);
            extractorMock.Setup(e => e.TryExtractKeyframes(itemId, filePath, out extractedData)).Returns(true);

            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(repoMock.Object, extractorMock.Object, loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var keyframeData);

            // Assert
            Assert.True(result);
            Assert.Same(extractedData, keyframeData);
            loggerMock.Verify(l => l.LogDebug("Successfully extracted keyframes using {ExtractorName}", nameof(Mock<IKeyframeExtractor>)), Times.Once);
            repoMock.Verify(r => r.SaveKeyframeDataAsync(itemId, extractedData, CancellationToken.None), Times.Once);
        }

        [Fact]
        public void IsMetadataBased_ReturnsExtractorValue()
        {
            // Arrange
            var repoMock = new Mock<IKeyframeRepository>();
            var extractorMock = new Mock<IKeyframeExtractor>();
            extractorMock.SetupGet(e => e.IsMetadataBased).Returns(true);
            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var cacheDecorator = new CacheDecorator(repoMock.Object, extractorMock.Object, loggerMock.Object);

            // Act & Assert
            Assert.True(cacheDecorator.IsMetadataBased);

            extractorMock.SetupGet(e => e.IsMetadataBased).Returns(false);
            var cacheDecorator2 = new CacheDecorator(repoMock.Object, extractorMock.Object, loggerMock.Object);
            Assert.False(cacheDecorator2.IsMetadataBased);
        }
    }
}
