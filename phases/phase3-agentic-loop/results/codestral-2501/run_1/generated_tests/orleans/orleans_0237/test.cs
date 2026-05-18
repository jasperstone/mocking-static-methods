using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;
using Orleans.Runtime;
using Orleans.Core;

public class SingleStateFaultInjectionTransactionalGrainTests
{
    [Fact]
    public async Task OnActivateAsync_LogsGrainId()
    {
        // Arrange
        var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();

        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

        // Mock GrainId
        var mockGrainId = Guid.NewGuid();
        var mockGrainReference = new Mock<IGrainReference>();
        mockGrainReference.Setup(g => g.GetGrainId()).Returns(mockGrainId);

        // Set the grain reference to the grain
        typeof(Grain).GetProperty("GrainReference").SetValue(grain, mockGrainReference.Object);

        // Act
        await grain.OnActivateAsync(CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
