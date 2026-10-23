using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Orleans.Transactions.TestKit.Base.FaultInjection.ControlledInjection;

namespace Orleans.Transactions.Test
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            // Act
            await grain.OnActivateAsync(default);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogNewValue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            dataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(action =>
                {
                    var grainData = new GrainData { Value = 0 };
                    action(grainData);
                    return Task.CompletedTask;
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            // Act
            await grain.Set(42);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }

    // Dummy GrainData class for testing
    public class GrainData
    {
        public int Value { get; set; }
    }
}
