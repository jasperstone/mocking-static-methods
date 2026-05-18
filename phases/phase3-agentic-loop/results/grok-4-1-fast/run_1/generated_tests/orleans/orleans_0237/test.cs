using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> _mockData;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<ILogger> _mockLogger;
        private readonly SingleStateFaultInjectionTransactionalGrain _grain;

        public SingleStateFaultInjectionTransactionalGrainTests()
        {
            _mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLogger = new Mock<ILogger>();

            _mockLoggerFactory
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(_mockLogger.Object);

            _grain = new SingleStateFaultInjectionTransactionalGrain(_mockData.Object, _mockLoggerFactory.Object);
        }

        [Fact]
        public async Task OnActivateAsync_LogsInformationWithGrainId()
        {
            // Act
            await _grain.OnActivateAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    It.IsAny<EventId>(),
                    "GrainId {GrainId}", 
                    It.IsAny<string>()
                ),
                Times.Once);
        }

        [Fact]
        public async Task Set_LogsInformationWithNewValue()
        {
            // Arrange
            var newValue = 42;
            _mockData
                .Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _grain.Set(newValue);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    It.IsAny<EventId>(),
                    "Setting value {NewValue}.",
                    newValue
                ),
                Times.Once);
        }

        [Fact]
        public async Task Add_LogsInformationWithNumberToAddAndCurrentValue()
        {
            // Arrange
            var numberToAdd = 10;
            var currentValue = 5;
            var mockData = new GrainData { Value = currentValue };

            _mockData
                .Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Callback<Action<GrainData>>(action => action(mockData))
                .Returns(Task.CompletedTask);

            // Act
            await _grain.Add(numberToAdd);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    It.IsAny<EventId>(),
                    "Adding {NumberToAdd} to value {Value}.",
                    numberToAdd,
                    currentValue
                ),
                Times.Once);
        }
    }
}
