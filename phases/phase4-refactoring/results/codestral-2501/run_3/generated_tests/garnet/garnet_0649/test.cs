using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

public class TsavoriteKVTests
{
    [Fact]
    public void GetLatestCheckpointTokens_LogsInformation_WhenNoIndexCheckpointFound()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TsavoriteKV<object, object, object, object>>>();
        var tsavoriteKV = new TsavoriteKV<object, object, object, object>
        {
            logger = mockLogger.Object
        };

        // Act
        tsavoriteKV.GetLatestCheckpointTokens(out _, out _, out _);

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
