using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task OnActivateAsync_LogsGrainId()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(
                Mock.Of<IFaultInjectionTransactionalState<GrainData>>(),
                loggerFactoryMock.Object);

            // Mock the GetGrainId method
            var grainMock = new Mock<Grain>();
            grainMock.Setup(g => g.GetGrainId()).Returns(Guid.NewGuid());
            grain.SetGrainRuntime(grainMock.Object);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("GrainId {GrainId}", It.IsAny<object[]>(), null), Times.Once);
        }

        [Fact]
        public async Task Set_LogsAndUpdatesValue()
        {
            // Arrange
            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            // Mock the GetGrainId method
            var grainMock = new Mock<Grain>();
            grainMock.Setup(g => g.GetGrainId()).Returns(Guid.NewGuid());
            grain.SetGrainRuntime(grainMock.Object);

            await grain.OnActivateAsync(CancellationToken.None);

            int newValue = 42;

            // Act
            await grain.Set(newValue);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Setting value {NewValue}.", It.IsAny<object[]>(), null), Times.Once);
            dataMock.Verify(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()), Times.Once);
        }

        [Fact]
        public async Task Add_LogsAndUpdatesValue()
        {
            // Arrange
            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            // Mock the GetGrainId method
            var grainMock = new Mock<Grain>();
            grainMock.Setup(g => g.GetGrainId()).Returns(Guid.NewGuid());
            grain.SetGrainRuntime(grainMock.Object);

            await grain.OnActivateAsync(CancellationToken.None);

            int numberToAdd = 10;

            // Act
            await grain.Add(numberToAdd);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Adding {NumberToAdd} to value {Value}.", It.IsAny<object[]>(), null), Times.Once);
            dataMock.Verify(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()), Times.Once);
        }
    }
}
