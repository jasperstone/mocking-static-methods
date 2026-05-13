using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;
using System.Threading.Tasks;
using System.Threading;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task Set_LogsInformation()
        {
            // Arrange
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Set(42);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value 42.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Add_LogsInformation()
        {
            // Arrange
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Add(10);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding 10 to value")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
