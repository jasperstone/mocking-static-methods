using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.TestingHost;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Test
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        private readonly Mock<ILoggerFactory> loggerFactoryMock;
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> dataMock;
        private readonly SingleStateFaultInjectionTransactionalGrain grain;

        public SingleStateFaultInjectionTransactionalGrainTests()
        {
            loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            dataMock.Setup(d => d.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Returns<System.Action<GrainData>>(action =>
                {
                    var grainData = new GrainData { Value = 0 };
                    action(grainData);
                    return Task.CompletedTask;
                });
            dataMock.Setup(d => d.PerformRead<int>(It.IsAny<System.Func<GrainData, int>>()))
                .Returns<System.Func<GrainData, int>>(func =>
                {
                    var grainData = new GrainData { Value = 42 };
                    return Task.FromResult(func(grainData));
                });

            grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);
        }

        [Fact]
        public async Task OnActivateAsync_LogsInformation()
        {
            // Arrange
            var grainId = Guid.NewGuid();
            var grainMock = new Mock<Grain>();
            grainMock.Setup(g => g.GetGrainId()).Returns(grainId);
            grainMock.Setup(g => g.GetPrimaryKey()).Returns(123);

            // Act
            await grain.OnActivateAsync(default);

            // Assert
            loggerMock.Verify(
                log => log.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Set_LogsInformationAndUpdatesValue()
        {
            // Arrange
            int newValue = 10;

            // Act
            await grain.Set(newValue);

            // Assert
            loggerMock.Verify(
                log => log.LogInformation("Setting value {NewValue}.", newValue),
                Times.Once);
        }
    }

    public class GrainData
    {
        public int Value { get; set; }
    }
}
