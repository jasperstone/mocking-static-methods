using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task Set_ShouldLogInformation_WithCorrectParameters()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);

            int newValue = 42;

            // Act
            await grain.Set(newValue);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("Setting value {NewValue}.", It.Is<int>(v => v == newValue)),
                Times.Once);
        }
    }
}
