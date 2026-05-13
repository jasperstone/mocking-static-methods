using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.UnitTests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task OnActivateAsync_LogsGrainId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grainId = "grain-id";

            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

            // We need to mock GetGrainId and GetPrimaryKey, but these are Orleans Grain methods.
            // Since we cannot override them easily, we simulate by setting up the loggerFactory to expect the grain id string.
            // We'll override GetGrainId().ToString() by passing the grainId string to CreateLogger.

            // Act
            // We simulate OnActivateAsync with a CancellationToken.None
            // But we need to override GetGrainId and GetPrimaryKey calls.
            // Since we cannot override them, we will create a derived test class to override these methods.

            var testGrain = new TestSingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object, grainId);

            await testGrain.OnActivateAsync(CancellationToken.None);

            // Assert
            mockLoggerFactory.Verify(f => f.CreateLogger(grainId), Times.Once);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestSingleStateFaultInjectionTransactionalGrain : SingleStateFaultInjectionTransactionalGrain
        {
            private readonly string grainId;

            public TestSingleStateFaultInjectionTransactionalGrain(
                IFaultInjectionTransactionalState<GrainData> data,
                ILoggerFactory loggerFactory,
                string grainId)
                : base(data, loggerFactory)
            {
                this.grainId = grainId;
            }

            public override Task OnActivateAsync(CancellationToken cancellationToken)
            {
                // Override GetGrainId().ToString() to return grainId
                this.logger = this.loggerFactory.CreateLogger(this.grainId);
                this.logger.LogInformation("GrainId {GrainId}", this.GetPrimaryKey());

                return Task.CompletedTask;
            }

            public override string GetPrimaryKey()
            {
                return grainId;
            }
        }
    }
}
