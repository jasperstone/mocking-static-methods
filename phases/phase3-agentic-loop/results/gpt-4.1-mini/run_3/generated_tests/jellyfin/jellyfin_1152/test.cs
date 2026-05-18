using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using MediaBrowser.Controller.Persistence;
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
        public void IsMetadataBased_ReturnsExtractorValue()
        {
            _keyframeExtractorMock.SetupGet(x => x.IsMetadataBased).Returns(true);
            Assert.True(_cacheDecorator.IsMetadataBased);

            _keyframeExtractorMock.SetupGet(x => x.IsMetadataBased).Returns(false);
            Assert.False(_cacheDecorator.IsMetadataBased);
        }

        [Fact]
        public void TryExtractKeyframes_ReturnsTrue_WhenKeyframeDataExistsInRepository()
        {
            var itemId = Guid.NewGuid();
            var keyframeData = new KeyframeData(1000, new List<long> { 10, 20, 30 });
            _keyframeRepositoryMock.Setup(x => x.GetKeyframeData(itemId))
                .Returns(new[] { keyframeData });

            var result = _cacheDecorator.TryExtractKeyframes(itemId, "file.mp4", out var outKeyframeData);

            Assert.True(result);
            Assert.Same(keyframeData, outKeyframeData);
            _keyframeExtractorMock.Verify(x => x.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData>.IsAny), Times.Never);
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
            _keyframeRepositoryMock.Setup(x => x.GetKeyframeData(itemId))
                .Returns(Array.Empty<KeyframeData>());

            KeyframeData? dummyOut = null;
            _keyframeExtractorMock.Setup(x => x.TryExtractKeyframes(itemId, "file.mp4", out dummyOut))
                .Returns(false);

            var result = _cacheDecorator.TryExtractKeyframes(itemId, "file.mp4", out var outKeyframeData);

            Assert.False(result);
            Assert.Null(outKeyframeData);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to extract keyframes using")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryExtractKeyframes_LogsDebugAndSavesData_WhenExtractorSucceeds()
        {
            var itemId = Guid.NewGuid();
            _keyframeRepositoryMock.Setup(x => x.GetKeyframeData(itemId))
                .Returns(Array.Empty<KeyframeData>());

            var extractedKeyframeData = new KeyframeData(2000, new List<long> { 5, 15, 25 });
            _keyframeExtractorMock.Setup(x => x.TryExtractKeyframes(itemId, "file.mp4", out extractedKeyframeData))
                .Returns(true);

            _keyframeRepositoryMock.Setup(x => x.SaveKeyframeDataAsync(itemId, extractedKeyframeData, CancellationToken.None))
                .Returns(System.Threading.Tasks.Task.CompletedTask)
                .Verifiable();

            var result = _cacheDecorator.TryExtractKeyframes(itemId, "file.mp4", out var outKeyframeData);

            Assert.True(result);
            Assert.Same(extractedKeyframeData, outKeyframeData);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully extracted keyframes using")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _keyframeRepositoryMock.Verify();
        }
    }
}
