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

            // We need to mock GetGrainId and GetPrimaryKey, but these are Orleans Grain methods.
            // We can override GetGrainId and GetPrimaryKey by subclassing for test or use reflection.
            // For simplicity, we will create a derived test class that overrides these methods.

            var testGrain = new TestGrain(mockData.Object, mockLoggerFactory.Object, "TestGrainId", 123);

            // Act
            await testGrain.OnActivateAsync(CancellationToken.None);

            // Assert
            mockLoggerFactory.Verify(f => f.CreateLogger("TestGrainId"), Times.Once);
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
            mockData.Setup(d => d.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Returns(Task.CompletedTask)
                .Callback<System.Action<GrainData>>(action =>
                {
                    var grainData = new GrainData();
                    action(grainData);
                });

            var testGrain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
            // Manually set logger to mockLogger to simulate OnActivateAsync called
            var loggerField = typeof(SingleStateFaultInjectionTransactionalGrain).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(testGrain, mockLogger.Object);

            // Act
            await testGrain.Set(42);

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

        private class TestGrain : SingleStateFaultInjectionTransactionalGrain
        {
            private readonly string grainId;
            private readonly long primaryKey;

            public TestGrain(IFaultInjectionTransactionalState<GrainData> data, ILoggerFactory loggerFactory, string grainId, long primaryKey)
                : base(data, loggerFactory)
            {
                this.grainId = grainId;
                this.primaryKey = primaryKey;
            }

            public override GrainId GetGrainId() => GrainId.Create("Test", grainId);

            public override long GetPrimaryKey() => primaryKey;
        }
    }
}
