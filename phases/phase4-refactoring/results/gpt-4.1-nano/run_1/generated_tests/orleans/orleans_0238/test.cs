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
        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(
                Mock.Of<IFaultInjectionTransactionalState<GrainData>>(),
                loggerFactoryMock.Object);

            // Act
            await grain.OnActivateAsync(default);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogNewValue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var mockState = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockState.Setup(s => s.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Returns<System.Action<GrainData>>(action =>
                {
                    var data = new GrainData { Value = 0 };
                    action(data);
                    return Task.CompletedTask;
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(
                mockState.Object,
                loggerFactoryMock.Object);

            // Act
            await grain.Set(42);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value 42.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
