using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;
using System.Threading.Tasks;
using Orleans.Runtime;
using System.Threading;

public class SingleStateFaultInjectionTransactionalGrainTests
{
    private class TestableSingleStateFaultInjectionTransactionalGrain : SingleStateFaultInjectionTransactionalGrain
    {
        public TestableSingleStateFaultInjectionTransactionalGrain(
            IFaultInjectionTransactionalState<GrainData> data,
            ILoggerFactory loggerFactory)
            : base(data, loggerFactory)
        {
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            this.logger = this.loggerFactory.CreateLogger("testGrain");
            this.logger.LogInformation("GrainId {GrainId}", "testGrain");

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Set_LogsInformation()
    {
        // Arrange
        var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var grain = new TestableSingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
        await grain.OnActivateAsync(default);

        // Act
        await grain.Set(42);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value 42.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Add_LogsInformation()
    {
        // Arrange
        var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var grain = new TestableSingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
        await grain.OnActivateAsync(default);

        // Act
        await grain.Add(10);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding 10 to value")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
