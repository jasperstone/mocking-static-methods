using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit.Base.FaultInjection.ControlledInjection;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task OnActivateAsync_LogsGrainId()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory.Object);

            // Act
            await grain.OnActivateAsync(default);

            // Assert
            logger.Verify(l => l.LogInformation("GrainId {GrainId}", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Set_LogsSetValue()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory.Object);
            await grain.OnActivateAsync(default);

            // Act
            await grain.Set(10);

            // Assert
            logger.Verify(l => l.LogInformation("Setting value {NewValue}.", 10), Times.Once);
        }

        [Fact]
        public async Task Add_LogsAddValue()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory.Object);
            await grain.OnActivateAsync(default);

            // Act
            await grain.Add(10);

            // Assert
            logger.Verify(l => l.LogInformation("Adding {NumberToAdd} to value {Value}.", 10, It.IsAny<object>()), Times.Once);
        }
    }
}
