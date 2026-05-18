using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Keyframes;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache
{
    public class CacheDecoratorTests
    {
        [Fact]
        public void TryExtractKeyframes_ShouldLogDebug_WhenExtractionFails()
        {
            // Arrange
            var keyframeRepositoryMock = new Mock<IKeyframeRepository>();
            var keyframeExtractorMock = new Mock<IKeyframeExtractor>();
            var loggerMock = new Mock<ILogger<CacheDecorator>>();

            var itemId = Guid.NewGuid();
            var filePath = "test/path";

            keyframeRepositoryMock.Setup(r => r.GetKeyframeData(itemId))
                .Returns(Enumerable.Empty<KeyframeData>());

            keyframeExtractorMock.Setup(e => e.TryExtractKeyframes(itemId, filePath, out It.Ref<KeyframeData>.IsAny))
                .Callback(new TryExtractCallback((Guid id, string path, out KeyframeData data) => { data = null; }))
                .Returns(false);

            var cacheDecorator = new CacheDecorator(
                keyframeRepositoryMock.Object,
                keyframeExtractorMock.Object,
                loggerMock.Object);

            // Act
            var result = cacheDecorator.TryExtractKeyframes(itemId, filePath, out var keyframeData);

            // Assert
            Assert.False(result);
            Assert.Null(keyframeData);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to extract keyframes using")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private delegate void TryExtractCallback(Guid id, string path, out KeyframeData data);
    }
}
