using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Orleans;
using Orleans.TestingHost;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Test
{
    public class SingleStateDeactivatingTransactionalGrainTests
    {
        private readonly Mock<ILoggerFactory> loggerFactoryMock;
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> dataMock;
        private readonly GrainActivationContextMock contextMock;
        private readonly SingleStateFaultInjectionTransactionalGrain grain;

        public SingleStateDeactivatingTransactionalGrainTests()
        {
            loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            contextMock = new GrainActivationContextMock();

            grain = new SingleStateFaultInjectionTransactionalGrain(dataMock.Object, loggerFactoryMock.Object);
            // Set up grain's context if needed
        }

        [Fact]
        public async Task OnActivateAsync_Should_LogGrainId()
        {
            // Arrange
            var grainId = Guid.NewGuid();
            var primaryKey = 123L;

            var grainMock = new Mock<Grain>();
            grainMock.Setup(g => g.GetGrainId()).Returns(grainId);
            grainMock.Setup(g => g.GetPrimaryKey()).Returns(primaryKey);
            grainMock.Setup(g => g.GetType()).Returns(typeof(SingleStateFaultInjectionTransactionalGrain));

            // Inject the mock context if necessary
            // For simplicity, assume grain can be instantiated directly

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                log => log.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"GrainId {grainId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
