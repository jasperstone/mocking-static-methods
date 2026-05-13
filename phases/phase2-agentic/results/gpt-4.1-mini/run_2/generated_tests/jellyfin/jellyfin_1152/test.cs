using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Mock<IKeyframeRepository> _keyframeRepositoryMock;
        private readonly Mock<IKeyframeExtractor> _keyframeExtractorMock;
        private readonly Mock<ILogger<CacheDecorator>> _loggerMock;
        private readonly CacheDecorator _cacheDecorator;

        public CacheDecoratorTests()
        {
            _keyframeRepositoryMock = new Mock<IKeyframeRepository>();
            _keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            _loggerMock = new Mock<ILogger<CacheDecorator>>();

            _cacheDecorator = new CacheDecorator(
                _keyframeRepositoryMock.Object,
                _keyframeExtractorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public void TryExtractKeyframes_ReturnsTrue_WhenKeyframeDataExistsInRepository()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "somefile.mp4";
            var existingKeyframeData = new KeyframeData(1000, new List<long> { 10, 20, 30 });
            _keyframeRepositoryMock.Setup(r => r.GetKeyframeData(itemId))
                .Returns(new[] { existingKeyframeData });

            // Act
            var result = _cacheDecorator.TryExtractKeyframes(itemId, filePath, out var keyframeData);

            // Assert
            Assert.True(result);
            Assert.Same(existingKeyframeData, keyframeData);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public void TryExtractKeyframes_LogsDebugAndReturnsFalse_WhenExtractorFails()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "somefile.mp4";
            _keyframeRepositoryMock.Setup(r => r.GetKeyframeData(itemId))
                .Returns(Enumerable.Empty<KeyframeData>());
            _keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData>.IsAny))
                .Returns(false);

            // Act
            var result = _cacheDecorator.TryExtractKeyframes(itemId, filePath, out var keyframeData);

            // Assert
            Assert.False(result);
            Assert.Null(keyframeData);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Failed to extract keyframes using CacheDecorator"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never); // Because the message template includes {ExtractorName}, the formatted message is different

            // Instead verify the extension method call with the expected message template and argument
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to extract keyframes using")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryExtractKeyframes_LogsDebugAndSavesData_WhenExtractorSucceeds()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var filePath = "somefile.mp4";
            _keyframeRepositoryMock.Setup(r => r.GetKeyframeData(itemId))
                .Returns(Enumerable.Empty<KeyframeData>());

            var extractedKeyframeData = new KeyframeData(2000, new List<long> { 15, 25, 35 });
            _keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(itemId, filePath, out extractedKeyframeData))
                .Returns(true);

            _keyframeRepositoryMock.Setup(r => r.SaveKeyframeDataAsync(itemId, extractedKeyframeData, CancellationToken.None))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            // Act
            var result = _cacheDecorator.TryExtractKeyframes(itemId, filePath, out var keyframeData);

            // Assert
            Assert.True(result);
            Assert.Same(extractedKeyframeData, keyframeData);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully extracted keyframes using")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _keyframeRepositoryMock.Verify(r => r.SaveKeyframeDataAsync(itemId, extractedKeyframeData, CancellationToken.None), Times.Once);
        }
    }
}
