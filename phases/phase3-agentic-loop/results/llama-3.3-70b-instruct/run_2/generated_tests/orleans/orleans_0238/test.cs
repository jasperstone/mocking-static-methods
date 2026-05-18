using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task Set_LogsInformation()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var dataMock = new Mock<IFaultInjectionTransactionalState<Orleans.Transactions.TestKit.GrainData>>();
            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactory);

            // Act
            await grain.Set(10);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task Add_LogsInformation()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var dataMock = new Mock<IFaultInjectionTransactionalState<Orleans.Transactions.TestKit.GrainData>>();
            dataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<Orleans.Transactions.TestKit.GrainData>>())).Callback((Action<Orleans.Transactions.TestKit.GrainData> action) => action(new Orleans.Transactions.TestKit.GrainData()));
            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactory);

            // Act
            await grain.Add(10);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
