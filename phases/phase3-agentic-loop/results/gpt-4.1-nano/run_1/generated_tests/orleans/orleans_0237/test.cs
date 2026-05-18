using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Test
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> mockData;
        private readonly Mock<ILoggerFactory> mockLoggerFactory;
        private readonly Mock<ILogger> mockLogger;
        private readonly SingleStateFaultInjectionTransactionalGrain grain;

        public SingleStateFaultInjectionTransactionalGrainTests()
        {
            mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
        }

        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var mockPrimaryKey = 123L;
            // Act
            await grain.OnActivateAsync(default);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("GrainId {GrainId}", It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogAndUpdateValue()
        {
            // Arrange
            var newValue = 42;
            mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Callback<Action<GrainData>>(action =>
                {
                    var data = new GrainData { Value = 10 };
                    action(data);
                })
                .Returns(Task.CompletedTask);

            // Act
            await grain.Set(newValue);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("Setting value {NewValue}.", newValue),
                Times.Once);
            mockData.Verify(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldLogAndAddValue()
        {
            // Arrange
            var initialData = new GrainData { Value = 5 };
            var numberToAdd = 3;
            mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Callback<Action<GrainData>>(action =>
                {
                    var data = initialData;
                    action(data);
                })
                .Returns(Task.CompletedTask);
            mockData.Setup(d => d.FaultInjectionControl).Returns(new FaultInjectionControl());

            // Act
            await grain.Add(numberToAdd);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("Adding {NumberToAdd} to value {Value}.", numberToAdd, initialData.Value),
                Times.Once);
            mockData.Verify(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()), Times.Once);
        }
    }

    // Dummy classes to support the test
    public class GrainData
    {
        public int Value { get; set; }
        public FaultInjectionControl FaultInjectionControl { get; } = new FaultInjectionControl();
    }

    public class FaultInjectionControl
    {
        public string FaultInjectionPhase { get; set; }
        public string FaultInjectionType { get; set; }

        public void Reset() { }
    }
}
