using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;
using System.Threading.Tasks;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task Set_LogsInformation()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var data = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new SingleStateFaultInjectionTransactionalGrain(data.Object, loggerFactory.Object);

            // Act
            await grain.Set(10);

            // Assert
            logger.Verify(l => l.LogInformation("Setting value {NewValue}.", 10), Times.Once);
        }

        [Fact]
        public async Task Add_LogsInformation()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var data = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            data.Setup(d => d.PerformRead<int>(It.IsAny<Func<GrainData, int>>())).Returns(5);
            var grain = new SingleStateFaultInjectionTransactionalGrain(data.Object, loggerFactory.Object);

            // Act
            await grain.Add(10);

            // Assert
            logger.Verify(l => l.LogInformation("Adding {NumberToAdd} to value {Value}.", 10, 5), Times.Once);
        }
    }
}
