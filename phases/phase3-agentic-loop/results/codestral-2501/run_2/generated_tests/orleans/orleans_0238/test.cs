using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.TestKit;
using Xunit;

public class SingleStateFaultInjectionTransactionalGrainTests
{
    [Fact]
    public async Task Set_LogsInformation()
    {
        // Arrange
        var mockState = new Mock<IFaultInjectionTransactionalState<GrainData>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var grain = new SingleStateFaultInjectionTransactionalGrain(mockState.Object, mockLoggerFactory.Object);

        var grainId = Guid.NewGuid();
        var mockGrainRuntime = new Mock<IGrainRuntime>();
        mockGrainRuntime.Setup(r => r.GrainId).Returns(grainId);

        var mockGrainContext = new Mock<IGrainContext>();
        mockGrainContext.Setup(c => c.GrainId).Returns(grainId);

        var mockGrain = new Mock<Grain>();
        mockGrain.Setup(g => g.Runtime).Returns(mockGrainRuntime.Object);
        mockGrain.Setup(g => g.GrainContext).Returns(mockGrainContext.Object);

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
        var mockState = new Mock<IFaultInjectionTransactionalState<GrainData>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var grain = new SingleStateFaultInjectionTransactionalGrain(mockState.Object, mockLoggerFactory.Object);

        var grainId = Guid.NewGuid();
        var mockGrainRuntime = new Mock<IGrainRuntime>();
        mockGrainRuntime.Setup(r => r.GrainId).Returns(grainId);

        var mockGrainContext = new Mock<IGrainContext>();
        mockGrainContext.Setup(c => c.GrainId).Returns(grainId);

        var mockGrain = new Mock<Grain>();
        mockGrain.Setup(g => g.Runtime).Returns(mockGrainRuntime.Object);
        mockGrain.Setup(g => g.GrainContext).Returns(mockGrainContext.Object);

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
