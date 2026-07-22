using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task OnActivateAsync_LogsGrainId()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger>();
            loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory);

            // Act
            await grain.OnActivateAsync(default);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task Set_LogsSetValue()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger>();
            loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory);

            // Act
            await grain.Set(10);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task Add_LogsAddValue()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger>();
            loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory);

            // Act
            await grain.Add(10);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
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
