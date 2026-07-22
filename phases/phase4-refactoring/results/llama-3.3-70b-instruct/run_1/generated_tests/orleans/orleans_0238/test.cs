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
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var grainDataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            grainDataMock.Setup(x => x.PerformUpdate(It.IsAny<Action<GrainData>>())).Callback((Action<GrainData> action) => action(new GrainData { Value = 0 }));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                grainDataMock.Object,
                loggerFactoryMock.Object);

            // Act
            await grain.Set(10);

            // Assert
            loggerMock.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task Add_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var grainDataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            grainDataMock.Setup(x => x.PerformUpdate(It.IsAny<Action<GrainData>>())).Callback((Action<GrainData> action) => action(new GrainData { Value = 0 }));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                grainDataMock.Object,
                loggerFactoryMock.Object);

            // Act
            await grain.Add(10);

            // Assert
            loggerMock.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
