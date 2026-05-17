using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;
using Orleans.Transactions.Abstractions;
using System.Threading.Tasks;
using System.Threading;
using Orleans.Runtime;
using Orleans;

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
            var mockGrainContext = new Mock<IGrainContext>();
            var mockGrainRuntime = new Mock<IGrainRuntime>();
            var mockGrainIdentity = new Mock<IGrainIdentity>();

            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            mockGrainContext.Setup(x => x.GrainId).Returns(mockGrainIdentity.Object);
            mockGrainContext.Setup(x => x.ActivationServices).Returns(new Mock<IServiceProvider>().Object);
            mockGrainRuntime.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockGrainContext.Object, mockGrainRuntime.Object, mockData.Object, mockLoggerFactory.Object);

            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Set(42);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
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
            var mockGrainContext = new Mock<IGrainContext>();
            var mockGrainRuntime = new Mock<IGrainRuntime>();
            var mockGrainIdentity = new Mock<IGrainIdentity>();

            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            mockGrainContext.Setup(x => x.GrainId).Returns(mockGrainIdentity.Object);
            mockGrainContext.Setup(x => x.ActivationServices).Returns(new Mock<IServiceProvider>().Object);
            mockGrainRuntime.Setup(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockGrainContext.Object, mockGrainRuntime.Object, mockData.Object, mockLoggerFactory.Object);

            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Add(10);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding 10 to value")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
