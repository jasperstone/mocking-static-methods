using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.TestingHost;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Test
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        private readonly Mock<ILoggerFactory> loggerFactoryMock;
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> dataMock;

        public SingleStateFaultInjectionTransactionalGrainTests()
        {
            loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
        }

        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);
            var grainId = "testGrainId";

            // Use reflection to set private fields if needed, or mock GetGrainId and GetPrimaryKey
            // For simplicity, assume these methods are virtual or accessible for mocking
            // Alternatively, you can create a derived class for testing that overrides these methods

            // Act
            await grain.OnActivateAsync(default);

            // Assert
            loggerMock.Verify(l => l.LogInformation("GrainId {GrainId}", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogAndUpdateValue()
        {
            // Arrange
            var newValue = 42;
            var data = new GrainData { Value = 0 };
            var performedUpdateCalled = false;

            dataMock.Setup(d => d.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Callback<System.Action<GrainData>>(action =>
                {
                    action(data);
                    performedUpdateCalled = true;
                })
                .Returns(Task.CompletedTask);

            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);
            // Inject a mock logger into the grain
            var logger = new Mock<ILogger>();
            typeof(SingleStateFaultInjectionTransactionalGrain)
                .GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(grain, logger.Object);

            // Act
            await grain.Set(newValue);

            // Assert
            Assert.True(performedUpdateCalled);
            Assert.Equal(newValue, data.Value);
            logger.Verify(l => l.LogInformation("Setting value {NewValue}.", newValue), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldLogAndAddValue()
        {
            // Arrange
            var numberToAdd = 10;
            var initialValue = 5;
            var data = new GrainData { Value = initialValue };
            var performedUpdateCalled = false;

            dataMock.Setup(d => d.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Callback<System.Action<GrainData>>(action =>
                {
                    action(data);
                    performedUpdateCalled = true;
                })
                .Returns(Task.CompletedTask);

            var grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);
            var logger = new Mock<ILogger>();
            typeof(SingleStateFaultInjectionTransactionalGrain)
                .GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(grain, logger.Object);

            // Act
            await grain.Add(numberToAdd);

            // Assert
            Assert.True(performedUpdateCalled);
            Assert.Equal(initialValue + numberToAdd, data.Value);
            logger.Verify(l => l.LogInformation("Adding {NumberToAdd} to value {Value}.", numberToAdd, initialValue), Times.Once);
        }
    }

    // Supporting class
    public class GrainData
    {
        public int Value { get; set; }
    }
}
