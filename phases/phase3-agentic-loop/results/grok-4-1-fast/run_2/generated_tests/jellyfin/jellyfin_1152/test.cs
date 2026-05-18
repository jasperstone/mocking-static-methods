using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Keyframes;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Cache.Tests
{
    public class CacheDecoratorTests
    {
        private readonly Mock<IKeyframeRepository> _mockKeyframeRepository;
        private readonly Mock<IKeyframeExtractor> _mockKeyframeExtractor;
        private readonly List<string> _logMessages;
        private readonly TestLogger<CacheDecorator> _logger;
        private readonly CacheDecorator _cacheDecorator;

        public CacheDecoratorTests()
        {
            _mockKeyframeRepository = new Mock<IKeyframeRepository>();
            _mockKeyframeExtractor = new Mock<IKeyframeExtractor>();
            _logMessages = new List<string>();
            _logger = new TestLogger<CacheDecorator>(_logMessages);

            _cacheDecorator = new CacheDecorator(
                _mockKeyframeRepository.Object,
                _mockKeyframeExtractor.Object,
                _logger);
        }

        [Fact]
        public void TryExtractKeyframes_CacheHit_ReturnsTrue()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var keyframeData = new KeyframeData(0, new List<long> { 1000, 2000 });
            _mockKeyframeRepository.Setup(r => r.GetKeyframeData(itemId))
                .Returns(new[] { keyframeData });

            KeyframeData? result;

            // Act
            var success = _cacheDecorator.TryExtractKeyframes(itemId, "test.mp4", out result);

            // Assert
            Assert.True(success);
            Assert.Same(keyframeData, result);
            Assert.Empty(_logMessages);
        }

        [Fact]
        public void TryExtractKeyframes_CacheMiss_ExtractorFails_LogsFailureMessage_ReturnsFalse()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _mockKeyframeRepository.Setup(r => r.GetKeyframeData(itemId))
                .Returns(Enumerable.Empty<KeyframeData>());

            KeyframeData? extracted = null;
            _mockKeyframeExtractor.Setup(e => e.TryExtractKeyframes(itemId, It.IsAny<string>(), out extracted))
                .Returns(false);

            KeyframeData? result;

            // Act
            var success = _cacheDecorator.TryExtractKeyframes(itemId, "test.mp4", out result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
            Assert.Contains("Failed to extract keyframes using", _logMessages);
            Assert.Single(_logMessages);
        }

        [Fact]
        public void TryExtractKeyframes_CacheMiss_ExtractorSucceeds_LogsSuccessMessage_ReturnsTrue()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var keyframeData = new KeyframeData(0, new List<long> { 1000, 2000 });
            _mockKeyframeRepository.Setup(r => r.GetKeyframeData(itemId))
                .Returns(Enumerable.Empty<KeyframeData>());

            KeyframeData? extracted = keyframeData;
            _mockKeyframeExtractor.Setup(e => e.TryExtractKeyframes(itemId, It.IsAny<string>(), out extracted))
                .Returns(true)
                .Callback((Guid _, string _, out KeyframeData? outData) => outData = keyframeData);

            _mockKeyframeRepository.Setup(r => r.SaveKeyframeDataAsync(itemId, keyframeData, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            KeyframeData? result;

            // Act
            var success = _cacheDecorator.TryExtractKeyframes(itemId, "test.mp4", out result);

            // Assert
            Assert.True(success);
            Assert.Same(keyframeData, result);
            Assert.Contains("Successfully extracted keyframes using", _logMessages);
            Assert.Single(_logMessages);
        }
    }

    public class TestLogger<T> : ILogger<T>
    {
        private readonly List<string> _logMessages;

        public TestLogger(List<string> logMessages)
        {
            _logMessages = logMessages;
        }

        public IDisposable? BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logMessages.Add(formatter(state, exception));
        }
    }
}
