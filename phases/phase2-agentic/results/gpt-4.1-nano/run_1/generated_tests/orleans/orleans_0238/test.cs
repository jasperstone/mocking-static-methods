using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Orleans;
using Orleans.TestingHost;
using Orleans.Hosting;
using Orleans.Transactions.Abstractions;
using Orleans.Runtime;
using Orleans.Core;
using Orleans.Configuration;

namespace Orleans.Transactions.Test
{
    public class SingleStateDeactivatingTransactionalGrainTests
    {
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> mockData;
        private readonly Mock<ILoggerFactory> mockLoggerFactory;
        private readonly Mock<ILogger> mockLogger;
        private readonly GrainActivationContextMock context;

        public SingleStateDeactivatingTransactionalGrainTests()
        {
            mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLogger = new Mock<ILogger>();
            context = new GrainActivationContextMock();

            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        }

        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
            var grainId = Guid.NewGuid();
            var primaryKey = 123L;

            var mockGrainContext = new Mock<IGrainContext>();
            mockGrainContext.Setup(c => c.GetPrimaryKey()).Returns(primaryKey);
            mockGrainContext.Setup(c => c.GetGrainId()).Returns(grainId);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"GrainId {grainId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogInformation()
        {
            // Arrange
            var grain = new SingleStateFaultInjectionTransactionalGrain(mockData.Object, mockLoggerFactory.Object);
            var newValue = 42;
            var mockD = new GrainData { Value = 0 };
            mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns<Action<GrainData>>(async action =>
                {
                    action(mockD);
                    await Task.CompletedTask;
                });

            // Act
            await grain.Set(newValue);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Setting value {newValue}.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mock class for GrainContext
    public class GrainActivationContextMock : IGrainContext
    {
        public Guid GrainId { get; set; } = Guid.NewGuid();
        public object ActivationServices => throw new NotImplementedException();
        public IGrainRuntime Runtime => throw new NotImplementedException();

        public T GetExtension<T>() where T : class => throw new NotImplementedException();

        public Guid GetGrainId() => GrainId;

        public long GetPrimaryKey() => 123L;

        public string GetGrainIdentityString() => GrainId.ToString();

        public void DeactivateOnIdle() { }

        public void RegisterTimer(Func<object, Task> callback, object state, TimeSpan dueTime, TimeSpan period) => throw new NotImplementedException();

        public void UnregisterTimer(IDisposable timer) => throw new NotImplementedException();

        public void Dispose() { }
    }

    // Dummy GrainData class
    public class GrainData
    {
        public int Value { get; set; }
        public FaultInjectionControl FaultInjectionControl { get; set; } = new FaultInjectionControl();
    }

    public class FaultInjectionControl
    {
        public string FaultInjectionPhase { get; set; }
        public string FaultInjectionType { get; set; }

        public void Reset()
        {
            FaultInjectionPhase = null;
            FaultInjectionType = null;
        }
    }
}
