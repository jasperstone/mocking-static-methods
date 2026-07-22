using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

public class IndexRecoveryTests
{
    [Fact]
    public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNotZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockOverflowBucketsAllocator = new Mock<OverflowBucketsAllocator>();
        var mockResizeInfo = new Mock<ResizeInfo>();
        var mockState = new Mock<State>();
        var mockHashBucket = new Mock<HashBucket>();
        var mockHashBucketEntry = new Mock<HashBucketEntry>();
        var mockConstants = new Mock<Constants>();
        var mockIDevice = new Mock<IDevice>();
        var mockHashIndexPageAsyncReadResult = new Mock<HashIndexPageAsyncReadResult>();

        var indexRecovery = new TsavoriteBase
        {
            logger = mockLogger.Object,
            overflowBucketsAllocator = mockOverflowBucketsAllocator.Object,
            resizeInfo = mockResizeInfo.Object,
            state = mockState.Object,
            HashBucket = mockHashBucket.Object,
            HashBucketEntry = mockHashBucketEntry.Object,
            Constants = mockConstants.Object,
            IDevice = mockIDevice.Object,
            HashIndexPageAsyncReadResult = mockHashIndexPageAsyncReadResult.Object
        };

        uint errorCode = 1;
        uint numBytes = 100;
        object overlap = new object();

        // Act
        indexRecovery.AsyncPageReadCallback(errorCode, numBytes, overlap);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

// Mock implementations for the dependencies
public class OverflowBucketsAllocator { }
public class ResizeInfo { }
public class State { }
public class HashBucket { }
public class HashBucketEntry { }
public class Constants { }
public interface IDevice { }
public class HashIndexPageAsyncReadResult { }
