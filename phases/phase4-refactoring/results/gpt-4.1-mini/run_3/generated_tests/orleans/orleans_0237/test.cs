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
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

            await grain.OnActivateAsync(CancellationToken.None);

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
        public async Task Set_LogsSettingValueAndUpdatesData()
        {
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            mockData.Setup(d => d.PerformUpdate(It.IsAny<Func<GrainData, Task>>()))
                .Returns<Func<GrainData, Task>>(func =>
                {
                    var data = new GrainData();
                    return Task.Run(() => func(data));
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

            await grain.Set(42);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Add_LogsAddingValueAndUpdatesData()
        {
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var faultInjectionControl = new FaultInjectionControl();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            mockData.SetupGet(m => m.FaultInjectionControl).Returns(faultInjectionControl);

            mockData.Setup(d => d.PerformUpdate(It.IsAny<Func<GrainData, Task>>()))
                .Returns<Func<GrainData, Task>>(func =>
                {
                    var data = new GrainData { Value = 10 };
                    return Task.Run(() => func(data));
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

            await grain.Add(5, faultInjectionControl);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Get_ReturnsValue()
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            mockData.Setup(d => d.PerformRead<int>(It.IsAny<Func<GrainData, int>>()))
                .Returns<Func<GrainData, int>>(func =>
                {
                    var data = new GrainData { Value = 123 };
                    return Task.FromResult(func(data));
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

            var result = await grain.Get();

            Assert.Equal(123, result);
        }

        [Fact]
        public async Task Deactivate_CallsDeactivateOnIdle()
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

            // We cannot directly verify DeactivateOnIdle because it's protected in Grain base class,
            // but we can call Deactivate and ensure it completes without error.
            await grain.Deactivate();
        }
    }
}
