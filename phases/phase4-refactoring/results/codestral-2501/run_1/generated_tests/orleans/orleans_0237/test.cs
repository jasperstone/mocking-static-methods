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

            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            // Mock GrainId
            var grainId = Guid.NewGuid();
            var grainMock = new Mock<Grain>();
            grainMock.Setup(g => g.GetGrainId()).Returns(grainId);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task Set_LogsAndUpdatesValue()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            // Act
            await grain.Set(42);

            // Assert
            loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
            dataMock.Verify(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()), Times.Once);
        }

        [Fact]
        public async Task Add_LogsAndUpdatesValue()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            // Act
            await grain.Add(10);

            // Assert
            loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
            dataMock.Verify(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()), Times.Once);
        }
    }
}
