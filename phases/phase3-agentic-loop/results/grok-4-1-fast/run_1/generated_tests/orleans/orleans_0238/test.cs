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
        public async Task Set_CallsLogInformationWithCorrectMessage()
        {
            // Arrange
            var newValue = 42;
            _mockData.Setup(d => d.PerformUpdate<Task>(It.IsAny<Func<GrainData, Task>>()))
                     .Returns(Task.CompletedTask);

            // Manually trigger OnActivateAsync to initialize logger
            await _grain.OnActivateAsync(default);

            // Act
            await _grain.Set(newValue);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    "Setting value {NewValue}.",
                    It.Is<int>(v => v == newValue)),
                Times.Once);
        }

        [Fact]
        public async Task Add_CallsLogInformationWithCorrectMessage()
        {
            // Arrange
            var numberToAdd = 10;
            var initialValue = 5;
            var mockGrainData = new GrainData { Value = initialValue };
            
            _mockData.SetupGet(d => d.FaultInjectionControl)
                     .Returns(new Mock<FaultInjectionControl>().Object);
            
            _mockData.Setup(d => d.PerformUpdate<Task>(It.IsAny<Func<GrainData, Task>>()))
                     .Callback<Func<GrainData, Task>>(func => func(mockGrainData))
                     .Returns(Task.CompletedTask);

            // Manually trigger OnActivateAsync to initialize logger
            await _grain.OnActivateAsync(default);

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
        public async Task OnActivateAsync_CallsLogInformationWithGrainId()
        {
            // Act
            await _grain.OnActivateAsync(default);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    "GrainId {GrainId}",
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
