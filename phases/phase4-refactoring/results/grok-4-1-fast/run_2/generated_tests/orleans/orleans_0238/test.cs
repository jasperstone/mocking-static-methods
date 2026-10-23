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
            _mockData.Setup(d => d.PerformUpdate<VoidResult>(It.IsAny<Action<GrainData>>()))
                     .Returns(Task.FromResult(default(VoidResult)));
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLogger = new Mock<ILogger>();

            _mockLoggerFactory
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(_mockLogger.Object);

            _grain = new SingleStateFaultInjectionTransactionalGrain(_mockData.Object, _mockLoggerFactory.Object);
        }

        [Fact]
        public async Task Set_CallsLogInformationWithCorrectMessage()
        {
            // Arrange
            var newValue = 42;

            // Manually trigger OnActivateAsync to initialize logger
            await _grain.OnActivateAsync(CancellationToken.None);

            // Act
            await _grain.Set(newValue);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    "Setting value {NewValue}.",
                    newValue),
                Times.Once);
        }

        [Fact]
        public async Task Add_CallsLogInformationWithCorrectMessage()
        {
            // Arrange
            var numberToAdd = 10;
            _mockData.SetupProperty(d => d.FaultInjectionControl);

            // Manually trigger OnActivateAsync to initialize logger
            await _grain.OnActivateAsync(CancellationToken.None);

            // Act
            await _grain.Add(numberToAdd);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    "Adding {NumberToAdd} to value {Value}.",
                    numberToAdd,
                    It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task OnActivateAsync_CreatesLoggerAndLogsInformation()
        {
            // Act
            await _grain.OnActivateAsync(CancellationToken.None);

            // Assert
            _mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
            _mockLogger.Verify(
                l => l.LogInformation(
                    "GrainId {GrainId}",
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
