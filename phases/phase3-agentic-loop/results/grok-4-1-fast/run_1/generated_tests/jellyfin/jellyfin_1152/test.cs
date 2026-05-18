using System;
using System.Collections.Generic;
using System.Linq;
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
    [Fact]
    public void TryExtractKeyframes_CacheHit_ReturnsTrue()
    {
        // Arrange
        var mockRepo = new Mock<IKeyframeRepository>();
        var mockExtractor = new Mock<IKeyframeExtractor>();
        var mockLogger = new Mock<ILogger<CacheDecorator>>();
        var keyframeData = new KeyframeData(0, new List<long>());

        mockRepo.Setup(r => r.GetKeyframeData(It.IsAny<Guid>())).Returns(new List<KeyframeData> { keyframeData });

        var decorator = new CacheDecorator(mockRepo.Object, mockExtractor.Object, mockLogger.Object);

        // Act
        var result = decorator.TryExtractKeyframes(Guid.NewGuid(), "test.mp4", out var extractedData);

        // Assert
        Assert.True(result);
        Assert.Equal(keyframeData, extractedData);
        mockLogger.VerifyNoOtherCalls();
    }

    [Fact]
    public void TryExtractKeyframes_CacheMiss_ExtractorFails_LogsFailureMessage()
    {
        // Arrange
        var mockRepo = new Mock<IKeyframeRepository>();
        var mockExtractor = new Mock<IKeyframeExtractor>();
        var mockLogger = new Mock<ILogger<CacheDecorator>>();
        var itemId = Guid.NewGuid();
        var extractorName = "TestExtractor";

        mockRepo.Setup(r => r.GetKeyframeData(itemId)).Returns(new List<KeyframeData>());
        mockExtractor.Setup(e => e.TryExtractKeyframes(It.IsAny<Guid>(), It.IsAny<string>(), out It.Ref<KeyframeData?>.IsAny))
                     .Returns(false);

        // Mock the inner extractor's type name via reflection setup
        var mockType = new Mock<Type>();
        mockType.Setup(t => t.Name).Returns(extractorName);
        mockExtractor.Setup(e => e.GetType()).Returns(mockType.Object);

        var decorator = new CacheDecorator(mockRepo.Object, mockExtractor.Object, mockLogger.Object);

        // Act
        var result = decorator.TryExtractKeyframes(itemId, "test.mp4", out var extractedData);

        // Assert
        Assert.False(result);
        Assert.Null(extractedData);
        
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to extract keyframes using") && v.ToString()!.Contains(extractorName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void TryExtractKeyframes_CacheMiss_ExtractorSucceeds_LogsSuccessMessage()
    {
        // Arrange
        var mockRepo = new Mock<IKeyframeRepository>();
        var mockExtractor = new Mock<IKeyframeExtractor>();
        var mockLogger = new Mock<ILogger<CacheDecorator>>();
        var itemId = Guid.NewGuid();
        var keyframeData = new KeyframeData(0, new List<long>());
        var extractorName = "TestExtractor";

        mockRepo.Setup(r => r.GetKeyframeData(itemId)).Returns(new List<KeyframeData>());
        mockExtractor.Setup(e => e.TryExtractKeyframes(itemId, It.IsAny<string>(), out keyframeData))
                     .Returns(true);

        var mockType = new Mock<Type>();
        mockType.Setup(t => t.Name).Returns(extractorName);
        mockExtractor.Setup(e => e.GetType()).Returns(mockType.Object);

        // Mock the repository save call
        mockRepo.Setup(r => r.SaveKeyframeDataAsync(It.IsAny<Guid>(), It.IsAny<KeyframeData>(), It.IsAny<CancellationToken>()))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

        var decorator = new CacheDecorator(mockRepo.Object, mockExtractor.Object, mockLogger.Object);

        // Act
        var result = decorator.TryExtractKeyframes(itemId, "test.mp4", out var extractedData);

        // Assert
        Assert.True(result);
        Assert.Equal(keyframeData, extractedData);
        
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully extracted keyframes using") && v.ToString()!.Contains(extractorName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void IsMetadataBased_DelegatesToInnerExtractor()
    {
        // Arrange
        var mockRepo = new Mock<IKeyframeRepository>();
        var mockExtractor = new Mock<IKeyframeExtractor>();
        var mockLogger = new Mock<ILogger<CacheDecorator>>();

        mockExtractor.Setup(e => e.IsMetadataBased).Returns(true);

        var decorator = new CacheDecorator(mockRepo.Object, mockExtractor.Object, mockLogger.Object);

        // Act
        var result = decorator.IsMetadataBased;

        // Assert
        Assert.True(result);
        mockExtractor.Verify(e => e.IsMetadataBased, Times.Once);
    }
}
