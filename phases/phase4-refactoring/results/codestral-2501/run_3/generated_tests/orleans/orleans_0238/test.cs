using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;
using System.Threading.Tasks;
using Orleans.Runtime;
using Orleans.Core;

public class SingleStateFaultInjectionTransactionalGrainTests
{
    [Fact]
    public async Task Set_LogsInformation()
    {
        // Arrange
        var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
        var mockGrainRuntime = new Mock<IGrainRuntime>();
        var mockGrainIdentity = new Mock<IGrainIdentity>();
        mockGrainIdentity.Setup(x => x.PrimaryKey).Returns(Guid.NewGuid());
        mockGrainRuntime.Setup(x => x.GrainIdentity).Returns(mockGrainIdentity.Object);
        grain.Runtime = mockGrainRuntime.Object;

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

        var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
        var mockGrainRuntime = new Mock<IGrainRuntime>();
        var mockGrainIdentity = new Mock<IGrainIdentity>();
        mockGrainIdentity.Setup(x => x.PrimaryKey).Returns(Guid.NewGuid());
        mockGrainRuntime.Setup(x => x.GrainIdentity).Returns(mockGrainIdentity.Object);
        grain.Runtime = mockGrainRuntime.Object;

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
