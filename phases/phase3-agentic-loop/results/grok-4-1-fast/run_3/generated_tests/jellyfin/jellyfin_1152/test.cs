using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Mock<IKeyframeRepository> _mockKeyframeRepository;
        private readonly Mock<IKeyframeExtractor> _mockKeyframeExtractor;
        private readonly Mock<ILogger<CacheDecorator>> _mockLogger;
        private readonly CacheDecorator _cacheDecorator;

        public CacheDecoratorTests()
        {
            _mockKeyframeRepository = new();
            _mockKeyframeExtractor = new();
            _mockLogger = new();

            _cacheDecorator = new CacheDecorator(
                _mockKeyframeRepository.Object,
                _mockKeyframeExtractor.Object,
                _mockLogger.Object);
        }

        [Fact]
        public void TryExtractKeyframes_CacheHit_ReturnsTrue()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var cachedData = new KeyframeData(1000, new List<long> { 0, 500 });
            _mockKeyframeRepository.Setup(r => r.GetKeyframeData(itemId))
                .Returns(new[] { cachedData }.ToList().AsReadOnly());

            KeyframeData? result;

            // Act
            var success = _cacheDecorator.TryExtractKeyframes(itemId, "test.mp4", out result);

            // Assert
            Assert.True(success);
            Assert.Same(cachedData, result);
            _mockLogger.VerifyNoOtherCalls();
        }

        [Fact]
        public void TryExtractKeyframes_CacheMiss_ExtractorFails_LogsFailureMessage()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _mockKeyframeRepository.Setup(r => r.GetKeyframeData(itemId))
                .Returns(Enumerable.Empty<KeyframeData>().ToList().AsReadOnly());
            
            KeyframeData? extractorResult = null;
            _mockKeyframeExtractor.Setup(e => e.TryExtractKeyframes(itemId, It.IsAny<string>(), out extractorResult))
                .Returns(false);

            KeyframeData? result;

            // Act
            var success = _cacheDecorator.TryExtractKeyframes(itemId, "test.mp4", out result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to extract keyframes using Mock")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryExtractKeyframes_CacheMiss_ExtractorSucceeds_LogsSuccessMessage()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var extractedData = new KeyframeData(1000, new List<long> { 0, 500 });
            _mockKeyframeRepository.Setup(r => r.GetKeyframeData(itemId))
                .Returns(Enumerable.Empty<KeyframeData>().ToList().AsReadOnly());
            
            KeyframeData? extractorResult = null;
            _mockKeyframeExtractor.Setup(e => e.TryExtractKeyframes(itemId, It.IsAny<string>(), out extractorResult))
                .Returns(true)
                .Callback((Guid id, string path, out KeyframeData? data) => { data = extractedData; });

            _mockKeyframeRepository.Setup(r => r.SaveKeyframeDataAsync(It.IsAny<Guid>(), It.IsAny<KeyframeData>(), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            KeyframeData? result;

            // Act
            var success = _cacheDecorator.TryExtractKeyframes(itemId, "test.mp4", out result);

            // Assert
            Assert.True(success);
            Assert.Same(extractedData, result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully extracted keyframes using Mock")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
