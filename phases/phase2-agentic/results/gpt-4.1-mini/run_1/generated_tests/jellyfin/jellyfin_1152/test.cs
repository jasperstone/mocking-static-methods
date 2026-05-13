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
        private readonly Guid _testItemId = Guid.NewGuid();
        private readonly string _testFilePath = "testfile.mp4";

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
        public void Constructor_ThrowsOnNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new CacheDecorator(null!, _keyframeExtractorMock.Object, _loggerMock.Object));
            Assert.Throws<ArgumentNullException>(() => new CacheDecorator(_keyframeRepositoryMock.Object, null!, _loggerMock.Object));
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
            var existingKeyframeData = new KeyframeData(1000, new List<long> { 10, 20, 30 });
            _keyframeRepositoryMock.Setup(x => x.GetKeyframeData(_testItemId))
                .Returns(new[] { existingKeyframeData });

            var result = _cacheDecorator.TryExtractKeyframes(_testItemId, _testFilePath, out var keyframeData);

            Assert.True(result);
            Assert.Same(existingKeyframeData, keyframeData);
            _keyframeExtractorMock.Verify(x => x.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData>.IsAny), Times.Never);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public void TryExtractKeyframes_LogsDebugAndReturnsFalse_WhenExtractorFails()
        {
            _keyframeRepositoryMock.Setup(x => x.GetKeyframeData(_testItemId))
                .Returns(Enumerable.Empty<KeyframeData>());

            KeyframeData? outData;
            _keyframeExtractorMock.Setup(x => x.TryExtractKeyframes(_testItemId, _testFilePath, out outData))
                .Returns(false);

            var result = _cacheDecorator.TryExtractKeyframes(_testItemId, _testFilePath, out var keyframeData);

            Assert.False(result);
            Assert.Null(keyframeData);

            // Verify LogDebug call with "Failed to extract keyframes using {ExtractorName}"
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to extract keyframes using")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryExtractKeyframes_LogsDebugAndSavesData_WhenExtractorSucceeds()
        {
            _keyframeRepositoryMock.Setup(x => x.GetKeyframeData(_testItemId))
                .Returns(Enumerable.Empty<KeyframeData>());

            var extractedKeyframeData = new KeyframeData(2000, new List<long> { 100, 200, 300 });
            KeyframeData? outData = extractedKeyframeData;
            _keyframeExtractorMock.Setup(x => x.TryExtractKeyframes(_testItemId, _testFilePath, out outData))
                .Returns(true);

            _keyframeRepositoryMock.Setup(x => x.SaveKeyframeDataAsync(_testItemId, extractedKeyframeData, CancellationToken.None))
                .Returns(System.Threading.Tasks.Task.CompletedTask)
                .Verifiable();

            var result = _cacheDecorator.TryExtractKeyframes(_testItemId, _testFilePath, out var keyframeData);

            Assert.True(result);
            Assert.Same(extractedKeyframeData, keyframeData);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully extracted keyframes using")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _keyframeRepositoryMock.Verify();
        }
    }
}
