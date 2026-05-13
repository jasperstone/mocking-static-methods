using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Cache.Tests
{
    public class CacheDecoratorTests
    {
        private readonly Mock<IKeyframeRepository> _repositoryMock;
        private readonly Mock<IKeyframeExtractor> _extractorMock;
        private readonly Mock<ILogger<CacheDecorator>> _loggerMock;
        private readonly CacheDecorator _cacheDecorator;
        private readonly Guid _testGuid = Guid.NewGuid();

        public CacheDecoratorTests()
        {
            _repositoryMock = new Mock<IKeyframeRepository>();
            _extractorMock = new Mock<IKeyframeExtractor>();
            _loggerMock = new Mock<ILogger<CacheDecorator>>();

            _extractorMock.Setup(e => e.IsMetadataBased).Returns(true);
            _extractorMock.Setup(e => e.GetType()).Returns(typeof(MockExtractor));
            _extractorMock.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData?>.IsAny))
                .Returns(false);

            _repositoryMock.Setup(r => r.GetKeyframeData(It.IsAny<Guid>()))
                .Returns(new List<KeyframeData>());

            _cacheDecorator = new CacheDecorator(_repositoryMock.Object, _extractorMock.Object, _loggerMock.Object);
        }

        private class MockExtractor : IKeyframeExtractor
        {
            public bool IsMetadataBased => true;
            public bool TryExtractKeyframes(Guid itemId, string filePath, out KeyframeData? keyframeData)
            {
                keyframeData = null;
                return false;
            }
        }

        [Fact]
        public void TryExtractKeyframes_ShouldLogDebug_WhenExtractionFails()
        {
            // Arrange
            _extractorMock.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData?>.IsAny))
                .Returns(false);

            // Act
            var result = _cacheDecorator.TryExtractKeyframes(_testGuid, "path", out var keyframeData);

            // Assert
            Assert.False(result);
            Assert.Null(keyframeData);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to extract keyframes using")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
