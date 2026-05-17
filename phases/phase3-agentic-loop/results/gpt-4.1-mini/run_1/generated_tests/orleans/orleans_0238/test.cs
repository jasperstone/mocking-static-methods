using System;
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
            // Arrange
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

            // We cannot call GetGrainId or GetPrimaryKey because they depend on Orleans runtime.
            // Instead, we simulate the logger creation and call OnActivateAsync, expecting no exceptions.
            // We verify that CreateLogger was called and LogInformation was called with expected message.

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Set_LogsSettingValue()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockData.Setup(d => d.PerformUpdate(It.IsAny<Func<GrainData, object>>()))
                .Returns<Func<GrainData, object>>(func =>
                {
                    var data = new GrainData();
                    func(data);
                    return Task.FromResult<object>(null);
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
            // Manually set logger to mockLogger to avoid calling OnActivateAsync
            var loggerField = typeof(SingleStateFaultInjectionTransactionalGrain).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(grain, mockLogger.Object);

            // Act
            await grain.Set(42);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
