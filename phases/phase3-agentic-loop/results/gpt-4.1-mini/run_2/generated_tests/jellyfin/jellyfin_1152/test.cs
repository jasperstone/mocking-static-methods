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
        private readonly Mock<IKeyframeRepository> _keyframeRepositoryMock;
        private readonly Mock<IKeyframeExtractor> _keyframeExtractorMock;
        private readonly Mock<ILogger<CacheDecorator>> _loggerMock;
        private readonly CacheDecorator _cacheDecorator;

        public CacheDecoratorTests()
        {
            _keyframeRepositoryMock = new Mock<IKeyframeRepository>();
            _keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            _loggerMock = new Mock<ILogger<CacheDecorator>>();

            _cacheDecorator = new CacheDecorator(_keyframeRepositoryMock.Object, _keyframeExtractorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void TryExtractKeyframes_ReturnsTrue_WhenKeyframeDataExistsInRepository()
        {
            var itemId = Guid.NewGuid();
            var keyframeData = new KeyframeData();
            _keyframeRepositoryMock.Setup(r => r.GetKeyframeData(itemId)).Returns(new[] { keyframeData });

            var result = _cacheDecorator.TryExtractKeyframes(itemId, "filePath", out var outKeyframeData);

            Assert.True(result);
            Assert.Equal(keyframeData, outKeyframeData);
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
            var itemId = Guid.NewGuid();
            _keyframeRepositoryMock.Setup(r => r.GetKeyframeData(itemId)).Returns(Array.Empty<KeyframeData>());
            _keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(itemId, "filePath", out It.Ref<KeyframeData>.IsAny))
                .Returns(false);

            var result = _cacheDecorator.TryExtractKeyframes(itemId, "filePath", out var outKeyframeData);

            Assert.False(result);
            Assert.Null(outKeyframeData);
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
            var itemId = Guid.NewGuid();
            var extractedKeyframeData = new KeyframeData();
            _keyframeRepositoryMock.Setup(r => r.GetKeyframeData(itemId)).Returns(Array.Empty<KeyframeData>());
            _keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(itemId, "filePath", out extractedKeyframeData))
                .Returns(true);

            var result = _cacheDecorator.TryExtractKeyframes(itemId, "filePath", out var outKeyframeData);

            Assert.True(result);
            Assert.Equal(extractedKeyframeData, outKeyframeData);
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
