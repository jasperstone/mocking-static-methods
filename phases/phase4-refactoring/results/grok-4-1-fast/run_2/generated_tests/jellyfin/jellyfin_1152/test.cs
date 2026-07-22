using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache
{
    public class CacheDecoratorTests
    {
        private readonly Mock<IKeyframeRepository> _mockRepository;
        private readonly Mock<IKeyframeExtractor> _mockExtractor;
        private readonly Mock<ILogger<CacheDecorator>> _mockLogger;
        private readonly CacheDecorator _cacheDecorator;

        public CacheDecoratorTests()
        {
            _mockRepository = new Mock<IKeyframeRepository>();
            _mockExtractor = new Mock<IKeyframeExtractor>();
            _mockLogger = new Mock<ILogger<CacheDecorator>>();

            _cacheDecorator = new CacheDecorator(
                _mockRepository.Object,
                _mockExtractor.Object,
                _mockLogger.Object);
        }

        [Fact]
        public void IsMetadataBased_ShouldReturnExtractorsValue()
        {
            // Arrange
            _mockExtractor.Setup(e => e.IsMetadataBased).Returns(true);

            // Act
            var result = _cacheDecorator.IsMetadataBased;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TryExtractKeyframes_WhenCacheHit_ShouldReturnTrue_WithoutCallingExtractor()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var cachedData = new KeyframeData(1000, new List<long> { 100, 200 });

            _mockRepository.Setup(r => r.GetKeyframeData(itemId))
                .Returns(new[] { cachedData });

            // Act
            var result = _cacheDecorator.TryExtractKeyframes(itemId, "/path/to/file.mp4", out var keyframeData);

            // Assert
            Assert.True(result);
            Assert.Equal(cachedData, keyframeData);
            _mockExtractor.Verify(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData?>.IsAny), Times.Never);
        }

        [Fact]
        public void TryExtractKeyframes_WhenCacheMissAndExtractorFails_ShouldLogFailure_AndReturnFalse()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "/path/to/file.mp4";

            _mockRepository.Setup(r => r.GetKeyframeData(itemId)).Returns(Enumerable.Empty<KeyframeData>());
            _mockExtractor.Setup(e => e.TryExtractKeyframes(itemId, filePath, out var result))
                .Returns(false);

            // Act
            var result = _cacheDecorator.TryExtractKeyframes(itemId, filePath, out var keyframeData);

            // Assert
            Assert.False(result);
            Assert.Null(keyframeData);
            _mockLogger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Failed to extract keyframes using {ExtractorName}") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            _mockRepository.Verify(r => r.SaveKeyframeDataAsync(It.IsAny<Guid>(), It.IsAny<KeyframeData>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void TryExtractKeyframes_WhenCacheMissAndExtractorSucceeds_ShouldLogSuccess_AndSaveToCache()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "/path/to/file.mp4";
            var extractedData = new KeyframeData(1000, new List<long> { 100, 200 });

            _mockRepository.Setup(r => r.GetKeyframeData(itemId)).Returns(Enumerable.Empty<KeyframeData>());
            _mockExtractor.Setup(e => e.TryExtractKeyframes(itemId, filePath, out var result))
                .Returns(true)
                .Callback<Guid, string, out KeyframeData?>((id, path, outResult) => { outResult = extractedData; });

            // Act
            var result = _cacheDecorator.TryExtractKeyframes(itemId, filePath, out var keyframeData);

            // Assert
            Assert.True(result);
            Assert.Equal(extractedData, keyframeData);
            _mockLogger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Successfully extracted keyframes using {ExtractorName}") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            _mockRepository.Verify(r => r.SaveKeyframeDataAsync(itemId, extractedData, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
