using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.TestingHost;
using Orleans.Hosting;
using Orleans.Transactions.Abstractions;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Test
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        private readonly Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>> _loggerMock;
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> _stateMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger> _innerLoggerMock;

        public SingleStateFaultInjectionTransactionalGrainTests()
        {
            _loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            _stateMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _innerLoggerMock = new Mock<ILogger>();

            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_innerLoggerMock.Object);
        }

        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var grain = new SingleStateFaultInjectionTransactionalGrain(_stateMock.Object, _loggerFactoryMock.Object);
            var mockGrainId = Guid.NewGuid();

            // Mock GetGrainId() extension method
            var mockRuntime = new Mock<IGrainRuntime>();
            mockRuntime.Setup(r => r.GetGrainId()).Returns(new GrainId(mockGrainId));
            // Use reflection or other means to inject mockRuntime if needed
            // For simplicity, assume GetGrainId() is accessible or replace with a testable pattern

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            _innerLoggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"GrainId {mockGrainId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogInformation()
        {
            // Arrange
            var grain = new SingleStateFaultInjectionTransactionalGrain(_stateMock.Object, _loggerFactoryMock.Object);
            var newValue = 42;

            _stateMock.Setup(s => s.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(async action =>
                {
                    var data = new GrainData { Value = 0 };
                    action(data);
                    return Task.CompletedTask;
                });

            // Act
            await grain.Set(newValue);

            // Assert
            _innerLoggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Setting value {newValue}.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Add_ShouldLogInformation()
        {
            // Arrange
            var grain = new SingleStateFaultInjectionTransactionalGrain(_stateMock.Object, _loggerFactoryMock.Object);
            var numberToAdd = 10;
            var initialValue = 5;

            _stateMock.Setup(s => s.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(async action =>
                {
                    var data = new GrainData { Value = initialValue };
                    action(data);
                    return Task.CompletedTask;
                });

            // Act
            await grain.Add(numberToAdd);

            // Assert
            _innerLoggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Adding {numberToAdd} to value {initialValue}.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy class for GrainData
    public class GrainData
    {
        public int Value { get; set; }
    }
}
