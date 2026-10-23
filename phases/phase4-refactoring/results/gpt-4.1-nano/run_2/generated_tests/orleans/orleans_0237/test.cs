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
        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockState = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new TestSingleStateFaultInjectionTransactionalGrain(mockState.Object, mockLoggerFactory.Object);

            // Act
            await grain.OnActivateAsync(default);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // A test subclass to override GetGrainId() for testing
        private class TestSingleStateFaultInjectionTransactionalGrain : SingleStateFaultInjectionTransactionalGrain
        {
            public TestSingleStateFaultInjectionTransactionalGrain(
                IFaultInjectionTransactionalState<GrainData> data,
                ILoggerFactory loggerFactory)
                : base(data, loggerFactory)
            {
            }

            public override GrainId GetGrainId()
            {
                // Return a dummy GrainId for testing
                return GrainId.NewId();
            }
        }
    }
}
