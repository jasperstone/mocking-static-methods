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
        private readonly Mock<IFaultInjectionTransactionalState<Orleans.Transactions.TestKit.GrainData>> _mockData;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<ILogger> _mockLogger;
        private readonly SingleStateFaultInjectionTransactionalGrain _grain;

        public SingleStateFaultInjectionTransactionalGrainTests()
        {
            _mockData = new Mock<IFaultInjectionTransactionalState<Orleans.Transactions.TestKit.GrainData>>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLogger = new Mock<ILogger>();

            _mockLoggerFactory
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(_mockLogger.Object);

            _mockData.Setup(d => d.FaultInjectionControl).Returns(new Mock<IFaultInjectionControl>().Object);

            _grain = new SingleStateFaultInjectionTransactionalGrain(_mockData.Object, _mockLoggerFactory.Object);
        }

        [Fact]
        public async Task Set_CallsLogInformationWithCorrectMessage()
        {
            // Arrange
            var newValue = 42;
            _mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<Orleans.Transactions.TestKit.GrainData>>()))
                     .Returns(Task.CompletedTask);

            // Initialize logger
            await _grain.OnActivateAsync(CancellationToken.None);

            // Act
            await _grain.Set(newValue);

            // Assert - tests line 50 LogInformation call
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
            var initialValue = 5;
            var grainData = new Orleans.Transactions.TestKit.GrainData { Value = initialValue };

            _mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<Orleans.Transactions.TestKit.GrainData>>()))
                     .Callback<Action<Orleans.Transactions.TestKit.GrainData>>(action => action(grainData))
                     .Returns(Task.CompletedTask);

            await _grain.OnActivateAsync(CancellationToken.None);

            // Act
            await _grain.Add(numberToAdd);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    "Adding {NumberToAdd} to value {Value}.",
                    numberToAdd,
                    initialValue),
                Times.Once);
        }

        [Fact]
        public async Task OnActivateAsync_CallsLogInformation()
        {
            // Act
            await _grain.OnActivateAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    "GrainId {GrainId}",
                    It.IsAny<object>()),
                Times.Once);
        }
    }
}
