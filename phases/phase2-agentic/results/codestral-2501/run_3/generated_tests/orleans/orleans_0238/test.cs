using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;

public class SingleStateFaultInjectionTransactionalGrainTests
{
    [Fact]
    public async Task Set_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
        var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

        await grain.OnActivateAsync(default);

        // Act
        await grain.Set(42);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value 42.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task Add_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
        var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

        await grain.OnActivateAsync(default);

        // Act
        await grain.Add(10);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding 10 to value")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
