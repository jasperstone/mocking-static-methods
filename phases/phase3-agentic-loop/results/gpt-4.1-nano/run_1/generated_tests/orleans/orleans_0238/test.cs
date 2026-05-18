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

            grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);
        }

        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var grainId = "testGrainId";
            var grainMock = new Mock<Grain>();
            grainMock.Setup(g => g.GetGrainId()).Returns(Guid.NewGuid());
            grainMock.Setup(g => g.GetPrimaryKey()).Returns(123);
            grainMock.Setup(g => g.GetType()).Returns(typeof(Grain));
            var grainInstance = grainMock.Object;

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                log => log.LogInformation("GrainId {GrainId}", It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogInformationAndUpdateValue()
        {
            // Arrange
            int newValue = 42;
            var data = new GrainData { Value = 0 };
            var performUpdateCalled = false;

            dataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(action =>
                {
                    performUpdateCalled = true;
                    action(data);
                    return Task.CompletedTask;
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            // Act
            await grain.Set(newValue);

            // Assert
            Assert.True(performUpdateCalled);
            Assert.Equal(newValue, data.Value);
            loggerMock.Verify(
                log => log.LogInformation("Setting value {NewValue}.", newValue),
                Times.Once);
        }
    }

    // Dummy class for GrainData
    public class GrainData
    {
        public int Value { get; set; }
        public FaultInjectionControl FaultInjectionControl { get; set; } = new FaultInjectionControl();
    }

    // Dummy class for FaultInjectionControl
    public class FaultInjectionControl
    {
        public string FaultInjectionPhase { get; set; }
        public string FaultInjectionType { get; set; }

        public void Reset() { }
    }
}
