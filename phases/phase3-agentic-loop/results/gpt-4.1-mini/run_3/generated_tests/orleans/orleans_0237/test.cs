using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
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
            var grainIdString = "grainIdString";
            var primaryKey = "primaryKey";

            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            // Create a test grain subclass that sets GrainId and simulates GetPrimaryKey call indirectly
            var grain = new TestGrain(mockData.Object, mockLoggerFactory.Object, grainIdString, primaryKey);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            mockLoggerFactory.Verify(f => f.CreateLogger(grainIdString), Times.Once);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        private class TestGrain : SingleStateFaultInjectionTransactionalGrain
        {
            private readonly string grainIdString;
            private readonly object primaryKey;

            public TestGrain(IFaultInjectionTransactionalState<GrainData> data, ILoggerFactory loggerFactory, string grainIdString, object primaryKey)
                : base(data, loggerFactory)
            {
                this.grainIdString = grainIdString;
                this.primaryKey = primaryKey;
            }

            // Override GetGrainId to return grainIdString.ToString()
            protected override GrainId GetGrainId() => GrainId.Create("test", grainIdString);

            // Override GetPrimaryKey to return primaryKey
            protected override object GetPrimaryKey() => primaryKey;
        }
    }
}
