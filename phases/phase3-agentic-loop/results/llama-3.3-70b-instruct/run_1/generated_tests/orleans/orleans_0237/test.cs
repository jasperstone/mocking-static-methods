using Xunit;
using Moq;
using Orleans.Transactions.TestKit;
using Microsoft.Extensions.Logging;

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
            loggerFactory.AddProvider(new TestLoggerProvider(loggerMock.Object));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory);
            grain.Activate(new Guid());

            // Act
            await grain.OnActivateAsync(default);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task Set_LogsSettingValue()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger>();
            loggerFactory.AddProvider(new TestLoggerProvider(loggerMock.Object));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory);
            grain.Activate(new Guid());

            // Act
            await grain.Set(10);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task Add_LogsAddingValue()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger>();
            loggerFactory.AddProvider(new TestLoggerProvider(loggerMock.Object));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                new Mock<IFaultInjectionTransactionalState<GrainData>>().Object,
                loggerFactory);
            grain.Activate(new Guid());

            // Act
            await grain.Add(10);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }

    public class TestLoggerProvider : ILoggerProvider
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
