using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.MediaEncoding.Hls.Cache;
using Jellyfin.MediaEncoding.Hls.Extractors;
using Jellyfin.MediaEncoding.Keyframes;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.MediaEncoding.Hls.Tests.Cache;

public class CacheDecoratorTests
{
    private readonly Mock<IKeyframeRepository> _mockRepository;
    private readonly Mock<IKeyframeExtractor> _mockExtractor;
    private readonly List<string> _logMessages;

    public CacheDecoratorTests()
    {
        _mockRepository = new Mock<IKeyframeRepository>();
        _mockExtractor = new Mock<IKeyframeExtractor>();
        _logMessages = new List<string>();
    }

    [Fact]
    public void TryExtractKeyframes_CacheHit_ReturnsTrue()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var totalDurationMs = 10000L;
        var keyframeTimes = new List<long> { 0, 5000, 10000 };
        var keyframeData = new KeyframeData(totalDurationMs, keyframeTimes);
        _mockRepository.Setup(r => r.GetKeyframeData(itemId)).Returns(new[] { keyframeData });

        var sut = CreateSut();

        // Act
        bool result = sut.TryExtractKeyframes(itemId, "test.mp4", out var actual);

        // Assert
        Assert.True(result);
        Assert.Equal(keyframeData, actual);
        _mockExtractor.Verify(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out var _), Times.Never);
    }

    [Fact]
    public void TryExtractKeyframes_CacheMiss_ExtractorFails_LogsFailure_ReturnsFalse()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetKeyframeData(itemId)).Returns(Enumerable.Empty<KeyframeData>());
        _mockExtractor.Setup(e => e.GetType()).Returns(typeof(FakeExtractor));
        _mockExtractor.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out KeyframeData? _))
            .Returns(false);

        var sut = CreateSut();

        // Act
        bool result = sut.TryExtractKeyframes(itemId, "test.mp4", out var _);

        // Assert
        Assert.False(result);
        Assert.Contains("Failed to extract keyframes using FakeExtractor", _logMessages);
    }

    [Fact]
    public void TryExtractKeyframes_CacheMiss_ExtractorSucceeds_LogsSuccess_ReturnsTrue()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var totalDurationMs = 10000L;
        var keyframeTimes = new List<long> { 0, 5000, 10000 };
        var extractedData = new KeyframeData(totalDurationMs, keyframeTimes);
        _mockRepository.Setup(r => r.GetKeyframeData(itemId)).Returns(Enumerable.Empty<KeyframeData>());
        _mockExtractor.Setup(e => e.GetType()).Returns(typeof(FakeExtractor));
        KeyframeData? outData = null;
        _mockExtractor.Setup(e => e.TryExtractKeyframes(It.Is<Guid>(g => g == itemId), It.IsAny<string>(), out outData))
            .Returns(true);
        _mockRepository.Setup(r => r.SaveKeyframeDataAsync(It.IsAny<Guid>(), It.IsAny<KeyframeData>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        bool result = sut.TryExtractKeyframes(itemId, "test.mp4", out var actual);

        // Assert
        Assert.True(result);
        Assert.Equal(extractedData, actual);
        Assert.Contains("Successfully extracted keyframes using FakeExtractor", _logMessages);
    }

    [Fact]
    public void IsMetadataBased_DelegatesToExtractor()
    {
        // Arrange
        _mockExtractor.Setup(e => e.IsMetadataBased).Returns(true);
        var sut = CreateSut();

        // Act
        bool result = sut.IsMetadataBased;

        // Assert
        Assert.True(result);
    }

    private CacheDecorator CreateSut()
    {
        var mockLogger = new Mock<ILogger<CacheDecorator>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Debug),
            It.IsAny<EventId>(),
            It.IsAny<object>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<object, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, id, state, ex, formatter) =>
            {
                _logMessages.Add(formatter(state, ex));
            });

        return new CacheDecorator(_mockRepository.Object, _mockExtractor.Object, mockLogger.Object);
    }
}
