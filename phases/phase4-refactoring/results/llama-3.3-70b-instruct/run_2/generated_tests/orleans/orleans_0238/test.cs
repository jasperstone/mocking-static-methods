using Xunit;
using Moq;
using Orleans.Transactions.TestKit;
using Microsoft.Extensions.Logging;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task Set_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            // Act
            await grain.Set(10);

            // Assert
            loggerMock.Verify(x => x.LogInformation("Setting value {NewValue}.", 10), Times.Once);
        }

        [Fact]
        public async Task Add_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            dataMock.Setup(x => x.PerformUpdate(It.IsAny<Action<GrainData>>())).Returns(Task.CompletedTask);
            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            // Act
            await grain.Add(10);

            // Assert
            loggerMock.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
