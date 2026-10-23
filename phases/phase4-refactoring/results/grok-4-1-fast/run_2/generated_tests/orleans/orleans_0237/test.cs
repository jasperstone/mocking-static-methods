using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
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

            _mockData
                .Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns(Task.CompletedTask);
            _mockData
                .Setup(d => d.PerformRead<int>(It.IsAny<Func<GrainData, int>>()))
                .ReturnsAsync(0);

            _grain = new SingleStateFaultInjectionTransactionalGrain(_mockData.Object, _mockLoggerFactory.Object);
        }

        [Fact]
        public async Task OnActivateAsync_CallsLogInformation_WithGrainId()
        {
            // Act
            await _grain.OnActivateAsync(CancellationToken.None);

            // Assert
            _mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
            _mockLogger.Verify(
                l => l.LogInformation(
                    "GrainId {GrainId}",
                    It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task Set_CallsLogInformation_WithNewValue()
        {
            // Arrange
            const int newValue = 42;

            // Act
            await _grain.Set(newValue);

            // Assert
            _mockData.Verify(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()), Times.Once);
            _mockLogger.Verify(
                l => l.LogInformation(
                    "Setting value {NewValue}.",
                    newValue),
                Times.Once);
        }

        [Fact]
        public async Task Add_CallsLogInformation_WithParameters()
        {
            // Arrange
            const int numberToAdd = 10;
            const int initialValue = 5;

            _mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Callback<Action<GrainData>>(action =>
                {
                    var data = new GrainData { Value = initialValue };
                    action(data);
                })
                .Returns(Task.CompletedTask);

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
    }
}
