using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using Orleans.TestingHost;
using Orleans.Transactions.TestKit;
using Orleans.Transactions.TestKit.Base.FaultInjection.ControlledInjection;

namespace Orleans.Transactions.Test
{
    public class SingleStateDeactivatingTransactionalGrainTests
    {
        private readonly Mock<ILoggerFactory> loggerFactoryMock;
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> dataMock;
        private readonly SingleStateFaultInjectionTransactionalGrain grain;

        public SingleStateDeactivatingTransactionalGrainTests()
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
            var grainId = Guid.NewGuid();
            var primaryKey = 123;
            var grainMock = new Mock<Grain>();
            grainMock.Setup(g => g.GetGrainId()).Returns(grainId);
            grainMock.Setup(g => g.GetPrimaryKey()).Returns(primaryKey);
            var grainInstance = grainMock.Object;

            // Act
            await grainInstance.OnActivateAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                log => log.LogInformation("GrainId {GrainId}", grainId.ToString()),
                Times.Once);
        }

        [Fact]
        public void Deactivate_ShouldCallDeactivateOnIdle()
        {
            // Arrange
            var deactivateCalled = false;
            var grainMock = new Mock<SingleStateFaultInjectionTransactionalGrain>(dataMock.Object, loggerFactoryMock.Object);
            grainMock.Setup(g => g.DeactivateOnIdle()).Callback(() => deactivateCalled = true);

            // Act
            var task = grainMock.Object.Deactivate();

            // Assert
            Assert.True(deactivateCalled);
            Assert.IsType<Task>(task);
        }

        [Fact]
        public async Task Set_ShouldLogInformation()
        {
            // Arrange
            var newValue = 42;
            var d = new GrainData { Value = 0 };
            var performUpdateCalled = false;

            var performUpdateMock = new Mock<Func<Action<GrainData>, Task>>();
            performUpdateMock.Setup(p => p(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(async action =>
                {
                    performUpdateCalled = true;
                    action(d);
                    await Task.CompletedTask;
                });

            dataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(async action =>
                {
                    performUpdateCalled = true;
                    action(d);
                    await Task.CompletedTask;
                });

            // Act
            await grain.Set(newValue);

            // Assert
            Assert.True(performUpdateCalled);
            Assert.Equal(newValue, d.Value);
            loggerMock.Verify(
                log => log.LogInformation("Setting value {NewValue}.", newValue),
                Times.Once);
        }
    }
}
