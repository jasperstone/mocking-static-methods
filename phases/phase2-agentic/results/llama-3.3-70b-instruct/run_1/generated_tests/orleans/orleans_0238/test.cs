using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit.Base.FaultInjection.ControlledInjection;

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
            loggerFactory.AddProvider(new TestLoggerProvider(loggerMock.Object));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory);

            // Act
            await grain.Set(10);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Setting value {NewValue}.", 10), Times.Once);
        }

        [Fact]
        public async Task Add_LogsInformation()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            loggerFactory.AddProvider(new TestLoggerProvider(loggerMock.Object));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory);

            // Act
            await grain.Add(10);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Adding {NumberToAdd} to value {Value}.", 10, 0), Times.Once);
        }

        private class TestLoggerProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public TestLoggerProvider(ILogger logger)
            {
                _logger = logger;
            }

            public ILogger CreateLogger(string categoryName)
            {
                return _logger;
            }

            public void Dispose()
            {
            }
        }
    }
}
