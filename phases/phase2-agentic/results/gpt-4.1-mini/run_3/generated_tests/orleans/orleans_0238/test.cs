using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> mockData;
        private readonly Mock<ILoggerFactory> mockLoggerFactory;
        private readonly Mock<ILogger> mockLogger;
        private readonly SingleStateFaultInjectionTransactionalGrain grain;

        public SingleStateFaultInjectionTransactionalGrainTests()
        {
            this.mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            this.mockLoggerFactory = new Mock<ILoggerFactory>();
            this.mockLogger = new Mock<ILogger>();

            this.mockLoggerFactory
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(mockLogger.Object);

            this.grain = new SingleStateFaultInjectionTransactionalGrain(this.mockData.Object, this.mockLoggerFactory.Object);

            // Setup GetGrainId and GetPrimaryKey for logger creation and logging
            // These are Orleans Grain methods, so we mock them by subclassing for test
        }

        private class TestGrain : SingleStateFaultInjectionTransactionalGrain
        {
            private readonly string grainId;
            private readonly object primaryKey;

            public TestGrain(
                IFaultInjectionTransactionalState<GrainData> data,
                ILoggerFactory loggerFactory,
                string grainId,
                object primaryKey)
                : base(data, loggerFactory)
            {
                this.grainId = grainId;
                this.primaryKey = primaryKey;
            }

            public override GrainId GetGrainId() => GrainId.Create("test", grainId);

            public override object GetPrimaryKey() => primaryKey;
        }

        [Fact]
        public async Task OnActivateAsync_LogsGrainId()
        {
            // Arrange
            var testGrain = new TestGrain(this.mockData.Object, this.mockLoggerFactory.Object, "grainId123", 42);

            // Act
            await testGrain.OnActivateAsync(CancellationToken.None);

            // Assert
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
            int newValue = 10;
            GrainData capturedData = null;
            mockData.Setup(d => d.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Returns<System.Action<GrainData>>(action =>
                {
                    var data = new GrainData();
                    action(data);
                    capturedData = data;
                    return Task.CompletedTask;
                });

            // Inject logger to grain
            var testGrain = new TestGrain(this.mockData.Object, this.mockLoggerFactory.Object, "grainId", 1);
            await testGrain.OnActivateAsync(CancellationToken.None);

            // Act
            await testGrain.Set(newValue);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(capturedData);
            Assert.Equal(newValue, capturedData.Value);
        }

        [Fact]
        public async Task Add_LogsAddingValue()
        {
            // Arrange
            int initialValue = 5;
            int numberToAdd = 3;
            var faultInjectionControl = new FaultInjectionControl();

            GrainData capturedData = new GrainData { Value = initialValue };
            mockData.SetupGet(d => d.FaultInjectionControl).Returns(faultInjectionControl);
            mockData.Setup(d => d.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Returns<System.Action<GrainData>>(action =>
                {
                    action(capturedData);
                    return Task.CompletedTask;
                });

            var testGrain = new TestGrain(this.mockData.Object, this.mockLoggerFactory.Object, "grainId", 1);
            await testGrain.OnActivateAsync(CancellationToken.None);

            // Act
            await testGrain.Add(numberToAdd, faultInjectionControl);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(initialValue + numberToAdd, capturedData.Value);
        }
    }

    // Minimal GrainData class for testing
    public class GrainData
    {
        public int Value { get; set; }
    }

    // Minimal FaultInjectionControl class for testing
    public class FaultInjectionControl
    {
        public int FaultInjectionPhase { get; set; }
        public int FaultInjectionType { get; set; }

        public void Reset()
        {
            FaultInjectionPhase = 0;
            FaultInjectionType = 0;
        }
    }

    // Minimal GrainId class for testing
    public class GrainId
    {
        private readonly string type;
        private readonly string key;

        private GrainId(string type, string key)
        {
            this.type = type;
            this.key = key;
        }

        public static GrainId Create(string type, string key) => new GrainId(type, key);

        public override string ToString() => $"{type}-{key}";
    }

    // Minimal Grain base class for testing
    public abstract class Grain
    {
        public virtual Task OnActivateAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public virtual GrainId GetGrainId() => GrainId.Create("default", "id");

        public virtual object GetPrimaryKey() => null;

        public void DeactivateOnIdle() { }
    }
}
