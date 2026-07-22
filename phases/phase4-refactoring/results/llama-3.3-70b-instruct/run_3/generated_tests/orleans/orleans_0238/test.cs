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
            loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
            var grainData = new GrainData { Value = 0 };
            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            dataMock.Setup(d => d.PerformUpdate(It.IsAny<Func<GrainData, GrainData>>()))
                .Callback<Func<GrainData, GrainData>>(f => f(grainData));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                dataMock.Object,
                loggerFactory);

            // Act
            await grain.OnActivateAsync(default);
            await grain.Set(10);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Setting value 10."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }

        [Fact]
        public async Task Add_LogsInformation()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger>();
            loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
            var grainData = new GrainData { Value = 0 };
            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            dataMock.Setup(d => d.PerformUpdate(It.IsAny<Func<GrainData, GrainData>>()))
                .Callback<Func<GrainData, GrainData>>(f => f(grainData));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                dataMock.Object,
                loggerFactory);

            // Act
            await grain.OnActivateAsync(default);
            await grain.Add(10);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Adding 10 to value 0."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }

        private class MockLoggerProvider : ILoggerProvider
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
}
