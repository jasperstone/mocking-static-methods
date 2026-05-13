using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class RemoteCommitServiceTests
    {
        [Fact]
        public async Task Fail_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "Test Data";

            // Act
            await service.Fail(transactionId, data);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Transaction {TransactionId} Failed with data: {Data}")),
                    It.Is<ILoggerEventId>(id => id == default),
                    It.Is<LogLevel>(level => level == LogLevel.Information),
                    It.Is<EventId>(id => id == default),
                    It.Is<Exception>(ex => ex == null),
                    It.Is<Func<ILogger, LogLevel, EventId, Exception, string>>(func => func(loggerMock.Object, LogLevel.Information, default, null, It.IsAny<object[]>()) == $"Transaction {transactionId} Failed with data: {data}"),
                    It.Is<object[]>(args => args[0] == transactionId && args[1] == data),
                    It.IsAny<ILoggerState>()
                ),
                Times.Once
            );
        }
    }
}
