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
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger>();
            loggerFactory.AddProvider(new TestLoggerProvider(loggerMock.Object));
            var grainDataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            grainDataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>())).Callback((Action<GrainData> action) => action(new GrainData()));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                grainDataMock.Object,
                loggerFactory);

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
            var loggerMock = new Mock<ILogger>();
            loggerFactory.AddProvider(new TestLoggerProvider(loggerMock.Object));
            var grainDataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            grainDataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>())).Callback((Action<GrainData> action) => action(new GrainData()));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                grainDataMock.Object,
                loggerFactory);

            // Act
            await grain.Add(10);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
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

    public class GrainData
    {
        public int Value { get; set; }
    }
}
