using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        private class GrainData
        {
            public int Value { get; set; }
        }

        private class FaultInjectionControl
        {
            public void Reset() { }
            public int FaultInjectionPhase { get; set; }
            public int FaultInjectionType { get; set; }
        }

        private interface IFaultInjectionTransactionalState<T>
        {
            FaultInjectionControl FaultInjectionControl { get; }
            Task PerformUpdate(Action<T> update);
            Task<TResult> PerformRead<TResult>(Func<T, TResult> read);
        }

        private class TestGrain : SingleStateFaultInjectionTransactionalGrain
        {
            public TestGrain(IFaultInjectionTransactionalState<GrainData> data, ILoggerFactory loggerFactory)
                : base(data, loggerFactory)
            {
            }

            // Expose logger for testing
            public ILogger Logger => this.GetType()
                .GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(this) as ILogger;
        }

        [Fact]
        public async Task OnActivateAsync_LogsGrainId()
        {
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            var grain = new TestGrain(mockData.Object, mockLoggerFactory.Object);

            // Setup GetGrainId and GetPrimaryKey by mocking Grain base class methods via reflection
            var grainId = Guid.NewGuid();
            var grainIdString = grainId.ToString();

            // We cannot override GetGrainId or GetPrimaryKey easily, so we simulate by setting logger manually
            // Instead, we call OnActivateAsync and verify logger usage

            await grain.OnActivateAsync(CancellationToken.None);

            mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
            mockLogger.Verify(l => l.Log(
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
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(action =>
                {
                    var data = new GrainData();
                    action(data);
                    return Task.CompletedTask;
                });

            var grain = new TestGrain(mockData.Object, mockLoggerFactory.Object);
            // Manually set logger to mockLogger to simulate OnActivateAsync
            var loggerField = typeof(SingleStateFaultInjectionTransactionalGrain).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(grain, mockLogger.Object);

            int newValue = 42;
            await grain.Set(newValue);

            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value") && v.ToString().Contains(newValue.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Add_LogsAddingValue()
        {
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockFaultInjectionControl = new Mock<FaultInjectionControl>();
            mockFaultInjectionControl.Setup(f => f.Reset());

            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockData.SetupGet(d => d.FaultInjectionControl).Returns(mockFaultInjectionControl.Object);
            mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(action =>
                {
                    var data = new GrainData { Value = 10 };
                    action(data);
                    return Task.CompletedTask;
                });

            var grain = new TestGrain(mockData.Object, mockLoggerFactory.Object);
            var loggerField = typeof(SingleStateFaultInjectionTransactionalGrain).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(grain, mockLogger.Object);

            int numberToAdd = 5;
            await grain.Add(numberToAdd);

            mockFaultInjectionControl.Verify(f => f.Reset(), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding") && v.ToString().Contains(numberToAdd.ToString()) && v.ToString().Contains("10")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
