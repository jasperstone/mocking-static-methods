using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.TestingHost;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        private readonly Mock<ILoggerFactory> loggerFactoryMock;
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> dataMock;
        private readonly SingleStateFaultInjectionTransactionalGrain grain;

        public SingleStateFaultInjectionTransactionalGrainTests()
        {
            loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);
        }

        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var grainId = Guid.NewGuid();
            var primaryKey = 123L;
            var grainMock = new Mock<IGrain>();
            grainMock.Setup(g => g.GetGrainId()).Returns(grainId);
            grainMock.Setup(g => g.GetPrimaryKey()).Returns(primaryKey);
            var grainInstance = grainMock.Object;

            // Act
            await grainInstance.OnActivateAsync(default);

            // Assert
            loggerMock.Verify(l => l.LogInformation("GrainId {GrainId}", grainId.ToString()), Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogInformation()
        {
            // Arrange
            var newValue = 42;
            var called = false;
            dataMock.Setup(d => d.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Callback<System.Action<GrainData>>(action =>
                {
                    var data = new GrainData { Value = 0 };
                    action(data);
                    called = true;
                })
                .Returns(Task.CompletedTask);

            // Act
            await grain.Set(newValue);

            // Assert
            Assert.True(called);
            loggerMock.Verify(l => l.LogInformation("Setting value {NewValue}.", newValue), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldLogInformation()
        {
            // Arrange
            var initialValue = 10;
            var numberToAdd = 5;
            var data = new GrainData { Value = initialValue };
            dataMock.Setup(d => d.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Callback<System.Action<GrainData>>(action =>
                {
                    action(data);
                })
                .Returns(Task.CompletedTask);

            // Act
            await grain.Add(numberToAdd);

            // Assert
            Assert.Equal(initialValue + numberToAdd, data.Value);
            loggerMock.Verify(l => l.LogInformation("Adding {NumberToAdd} to value {Value}.", numberToAdd, initialValue), Times.Once);
        }
    }

    public class GrainData
    {
        public int Value { get; set; }
    }
}
