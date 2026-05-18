using System;
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

            var mockState = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockState.Setup(s => s.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(action =>
                {
                    var data = new GrainData();
                    action(data);
                    return Task.CompletedTask;
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockState.Object, mockLoggerFactory.Object);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);
            await grain.Set(42);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value 42.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
