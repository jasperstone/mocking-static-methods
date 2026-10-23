using System;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

public class TsavoriteBaseTests
{
    [Fact]
    public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNotZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TsavoriteBase>>();
        var countdownWrapperMock = new Mock<CountdownWrapper>(1, false);
        var tsavoriteBase = new TsavoriteBase
        {
            logger = loggerMock.Object,
            recoveryCountdown = countdownWrapperMock.Object
        };

        uint errorCode = 1;
        uint numBytes = 100;
        object overlap = new object();

        // Act
        tsavoriteBase.AsyncPageReadCallback(errorCode, numBytes, overlap);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
