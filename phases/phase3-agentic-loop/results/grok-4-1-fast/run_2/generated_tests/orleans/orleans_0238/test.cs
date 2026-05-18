using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.FaultInjection.ControlledInjection
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

            _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);

            _grain = new SingleStateFaultInjectionTransactionalGrain(_mockData.Object, _mockLoggerFactory.Object);
        }

        [Fact]
        public async Task Set_CallsLogInformationWithCorrectMessage()
        {
            // Arrange
            const int newValue = 42;
            _mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>())).Returns(Task.CompletedTask);

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
            const int numberToAdd = 10;
            _mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>())).Returns(Task.CompletedTask);

            // Act
            await _grain.Add(numberToAdd);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("Adding {NumberToAdd} to value {Value}.")),
                    It.IsAny<object>(),
                    It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task OnActivateAsync_CallsLogInformation()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await _grain.OnActivateAsync(cancellationToken);

            // Assert
            _mockLogger.Verify(
                l => l.LogInformation(
                    "GrainId {GrainId}",
                    It.IsAny<object>()),
                Times.Once);
        }
    }

    public class GrainData
    {
        public int Value { get; set; }
    }
}
