using System;
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
            var grainId = Guid.NewGuid();

            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new TestSingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object, grainId, mockLogger);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
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
            private readonly Guid grainId;
            private readonly ILogger testLogger;

            public TestSingleStateFaultInjectionTransactionalGrain(
                IFaultInjectionTransactionalState<GrainData> data,
                ILoggerFactory loggerFactory,
                Guid grainId,
                ILogger logger)
                : base(data, loggerFactory)
            {
                this.grainId = grainId;
                this.testLogger = logger;
            }

            public override Task OnActivateAsync(CancellationToken cancellationToken)
            {
                // Instead of calling base, set logger and log manually to simulate base behavior
                // because base calls GetGrainId and GetPrimaryKey which are not virtual or accessible
                var loggerField = typeof(SingleStateFaultInjectionTransactionalGrain)
                    .GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                loggerField.SetValue(this, testLogger);

                testLogger.LogInformation("GrainId {GrainId}", grainId);

                return Task.CompletedTask;
            }
        }
    }
}
