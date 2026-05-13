using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Base.FaultInjection.ControlledInjection.Tests
{
    public class SingleStateDeactivatingTransactionalGrainTests
    {
        [Fact]
        public async Task OnActivateAsync_LogsGrainId()
        {
            // Arrange
            var grainId = new GrainId("test-grain-id");
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(
                null, // Mock or stub IFaultInjectionTransactionalState<GrainData> as needed
                loggerFactoryMock.Object);

            // Set the grain's ID for testing
            var grainIdProperty = typeof(Grain).GetProperty("GrainId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            grainIdProperty.SetValue(grain, grainId);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("GrainId {GrainId}", grainId),
                Times.Once);
        }
    }
}
