using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;
using System;
using System.Threading.Tasks;

public class RemoteCommitServiceTests
{
    [Fact]
    public async Task Fail_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RemoteCommitService>>();
        var remoteCommitService = new RemoteCommitService(loggerMock.Object);
        var transactionId = Guid.NewGuid();
        var data = "test data";

        // Act
        var result = await remoteCommitService.Fail(transactionId, data);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Transaction {transactionId} Failed with data: {data}")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
        Assert.False(result);
    }
}
