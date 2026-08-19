using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;
using System.Threading.Tasks;
using System.Threading;
using Orleans.Runtime;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task Set_LogsInformation()
        {
            // Arrange
            var mockState = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockState.Object, mockLoggerFactory.Object);
            var mockGrain = new Mock<SingleStateFaultInjectionTransactionalGrain>(mockState.Object, mockLoggerFactory.Object);
            mockGrain.Setup(x => x.GetGrainId()).Returns(new GrainId(GrainType.Create("testGrain"), IdSpan.Create("testGrain")));
            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Set(42);

            // Assert
            mockLogger.Verify(x => x.LogInformation("Setting value {NewValue}.", 42), Times.Once);
        }

        [Fact]
        public async Task Add_LogsInformation()
        {
            // Arrange
            var mockState = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockState.Object, mockLoggerFactory.Object);
            var mockGrain = new Mock<SingleStateFaultInjectionTransactionalGrain>(mockState.Object, mockLoggerFactory.Object);
            mockGrain.Setup(x => x.GetGrainId()).Returns(new GrainId(GrainType.Create("testGrain"), IdSpan.Create("testGrain")));
            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Add(10);

            // Assert
            mockLogger.Verify(x => x.LogInformation("Adding {NumberToAdd} to value {Value}.", 10, It.IsAny<int>()), Times.Once);
        }
    }
}
