using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Orleans;
using Orleans.TestingHost;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Test
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(
                Mock.Of<IFaultInjectionTransactionalState<GrainData>>(),
                loggerFactoryMock.Object);

            var grainId = Guid.NewGuid();
            var grainMock = new Mock<Grain>();
            grainMock.Setup(g => g.GetGrainId()).Returns(grainId);
            grainMock.Setup(g => g.GetPrimaryKey()).Returns(123);

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

        [Fact]
        public async Task Set_ShouldLogInformation_WhenCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SingleStateFaultInjectionTransactionalGrain>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var performUpdateCalled = false;
            dataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(action =>
                {
                    performUpdateCalled = true;
                    var grainData = new GrainData { Value = 42 };
                    action(grainData);
                    return Task.CompletedTask;
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(
                dataMock.Object,
                loggerFactoryMock.Object);

            // Act
            await grain.Set(99);

            // Assert
            Assert.True(performUpdateCalled);
            loggerMock.Verify(
                log => log.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value 99.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy class for GrainData
    public class GrainData
    {
        public int Value { get; set; }
    }
}
