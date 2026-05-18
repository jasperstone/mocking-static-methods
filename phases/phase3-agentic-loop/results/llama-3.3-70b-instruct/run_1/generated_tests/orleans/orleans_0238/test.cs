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
            grainDataMock.SetupGet(d => d.FaultInjectionControl).Returns(new FaultInjectionControl());
            grainDataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Callback<Action<GrainData>>(a => a(new GrainData { Value = 0 }));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                grainDataMock.Object,
                loggerFactory);

            // Act
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
            loggerFactory.AddProvider(new TestLoggerProvider(loggerMock.Object));
            var grainDataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            grainDataMock.SetupGet(d => d.FaultInjectionControl).Returns(new FaultInjectionControl());
            grainDataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Callback<Action<GrainData>>(a => a(new GrainData { Value = 0 }));
            var grain = new SingleStateFaultInjectionTransactionalGrain(
                grainDataMock.Object,
                loggerFactory);

            // Act
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
