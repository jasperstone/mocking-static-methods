using System;
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
        public async Task Set_LogsInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

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
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockData.Setup(x => x.PerformUpdate(It.IsAny<Action<GrainData>>())).Callback<Action<GrainData>>(action =>
            {
                var data = new GrainData { Value = 10 };
                action(data);
            });

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

            // Act
            await grain.Add(5);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding 5 to value 10.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
