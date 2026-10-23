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
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();

            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrainForTest(mockData.Object, mockLoggerFactory.Object);

            await grain.OnActivateAsync(CancellationToken.None);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        private class SingleStateFaultInjectionTransactionalGrainForTest : SingleStateFaultInjectionTransactionalGrain
        {
            private readonly ILoggerFactory _loggerFactory;

            public SingleStateFaultInjectionTransactionalGrainForTest(
                IFaultInjectionTransactionalState<GrainData> data,
                ILoggerFactory loggerFactory)
                : base(data, loggerFactory)
            {
                _loggerFactory = loggerFactory;
            }

            public override Task OnActivateAsync(CancellationToken cancellationToken)
            {
                var logger = _loggerFactory.CreateLogger("TestGrainId");
                logger.LogInformation("GrainId {GrainId}", 42L);
                return Task.CompletedTask;
            }
        }
    }
}
