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
        [Fact]
        public async Task OnActivateAsync_LogsGrainId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);

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
        public async Task Set_LogsSettingValueAndUpdatesData()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grainData = new GrainData();
            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockData.Setup(d => d.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Returns<System.Action<GrainData>>(action =>
                {
                    action(grainData);
                    return Task.CompletedTask;
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
            // Manually set logger since OnActivateAsync is not called here
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

            Assert.Equal(42, grainData.Value);
        }

        [Fact]
        public async Task Add_LogsAddingValueAndUpdatesData()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var grainData = new GrainData { Value = 10 };
            var faultInjectionControl = new FaultInjectionControl();

            var mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockData.SetupGet(d => d.FaultInjectionControl).Returns(faultInjectionControl);
            mockData.Setup(d => d.PerformUpdate(It.IsAny<System.Action<GrainData>>()))
                .Returns<System.Action<GrainData>>(action =>
                {
                    action(grainData);
                    return Task.CompletedTask;
                });

            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
            // Manually set logger since OnActivateAsync is not called here
            var loggerField = typeof(SingleStateFaultInjectionTransactionalGrain).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(grain, mockLogger.Object);

            // Act
            await grain.Add(5, new FaultInjectionControl { FaultInjectionPhase = 1, FaultInjectionType = 2 });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(15, grainData.Value);
            Assert.Equal(1, faultInjectionControl.FaultInjectionPhase);
            Assert.Equal(2, faultInjectionControl.FaultInjectionType);
        }
    }

    // Minimal stub classes to support the tests
    public class GrainData
    {
        public int Value { get; set; }
    }

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

    public interface IFaultInjectionTransactionalState<T>
    {
        FaultInjectionControl FaultInjectionControl { get; }
        Task PerformUpdate(System.Action<T> update);
        Task<TResult> PerformRead<TResult>(System.Func<T, TResult> read);
    }

    // Minimal Grain and IGrainWithGuidKey stubs to allow compilation
    public abstract class Grain
    {
        public virtual Task OnActivateAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public void DeactivateOnIdle() { }
        public object GetGrainId() => "grainId";
        public object GetPrimaryKey() => "primaryKey";
    }

    public interface IGrainWithGuidKey { }
}
