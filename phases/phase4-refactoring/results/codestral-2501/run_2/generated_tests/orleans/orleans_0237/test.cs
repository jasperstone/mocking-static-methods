using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task OnActivateAsync_LogsGrainId()
        {
            // Arrange
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new TestSingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Set_LogsNewValue()
        {
            // Arrange
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new TestSingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Set(42);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value 42")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Add_LogsNumberToAddAndValue()
        {
            // Arrange
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new TestSingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
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

        private class TestSingleStateFaultInjectionTransactionalGrain : SingleStateFaultInjectionTransactionalGrain
        {
            public TestSingleStateFaultInjectionTransactionalGrain(
                IFaultInjectionTransactionalState<GrainData> data,
                ILoggerFactory loggerFactory)
                : base(data, loggerFactory)
            {
            }

            public override Guid GetGrainId()
            {
                return Guid.NewGuid();
            }
        }
    }
}
