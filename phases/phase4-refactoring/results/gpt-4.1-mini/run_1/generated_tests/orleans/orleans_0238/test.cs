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
        [Fact]
        public async Task Set_LogsInformationWithNewValue()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grainData = new GrainData();
            var mockState = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockState.Setup(s => s.PerformUpdate<object>(It.IsAny<System.Func<GrainData, object>>()))
                .Returns<System.Func<GrainData, object>>(updateFunc =>
                {
                    updateFunc(grainData);
                    return Task.FromResult<object>(null);
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockState.Object, mockLoggerFactory.Object);

            // Manually set logger to mockLogger to simulate OnActivateAsync behavior
            var loggerField = typeof(SingleStateFaultInjectionTransactionalGrain).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(grain, mockLogger.Object);

            // Act
            await grain.Set(42);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value 42.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
            Assert.Equal(42, grainData.Value);
        }

        [Fact]
        public async Task OnActivateAsync_LogsGrainId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockState = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new SingleStateFaultInjectionTransactionalGrain(mockState.Object, mockLoggerFactory.Object);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
