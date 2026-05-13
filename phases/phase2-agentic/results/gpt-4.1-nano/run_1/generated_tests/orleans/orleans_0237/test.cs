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
using Orleans.Configuration;
using Orleans.TestingHost.Utils;

namespace Orleans.Transactions.Test
{
    public class SingleStateDeactivatingTransactionalGrainTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> _stateMock;
        private readonly GrainActivationContextMock _activationContext;
        private readonly SingleStateFaultInjectionTransactionalGrain _grain;

        public SingleStateDeactivatingTransactionalGrainTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

            _stateMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            _stateMock.Setup(s => s.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns((Action<GrainData> action) =>
                {
                    var data = new GrainData { Value = 0 };
                    action(data);
                    return Task.CompletedTask;
                });
            _stateMock.Setup(s => s.PerformRead<int>(It.IsAny<Func<GrainData, int>>()))
                .Returns((Func<GrainData, int> func) =>
                {
                    var data = new GrainData { Value = 42 };
                    return Task.FromResult(func(data));
                });

            _activationContext = new GrainActivationContextMock();

            _grain = new SingleStateFaultInjectionTransactionalGrain(_stateMock.Object, _loggerFactoryMock.Object);
            // Manually set the logger to simulate OnActivateAsync
            _grain.GetType().GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_grain, _loggerMock.Object);
        }

        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var grainId = Guid.NewGuid();
            var primaryKey = 123L;
            var grain = new SingleStateFaultInjectionTransactionalGrain(_stateMock.Object, _loggerFactoryMock.Object);
            // Mock GetGrainId and GetPrimaryKey
            var grainType = typeof(SingleStateFaultInjectionTransactionalGrain);
            var getGrainIdMethod = grainType.GetMethod("GetGrainId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var getPrimaryKeyMethod = grainType.GetMethod("GetPrimaryKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Since these are not public, we simulate the call by setting the logger directly as in constructor

            // Act
            await grain.OnActivateAsync(default);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("GrainId {GrainId}", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogInformation()
        {
            // Arrange
            int newValue = 10;

            // Act
            await _grain.Set(newValue);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("Setting value {NewValue}.", newValue), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldLogInformation()
        {
            // Arrange
            int numberToAdd = 5;
            var initialData = new GrainData { Value = 10 };
            _stateMock.Setup(s => s.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Returns((Action<GrainData> action) =>
                {
                    action(initialData);
                    return Task.CompletedTask;
                });

            // Act
            await _grain.Add(numberToAdd);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("Adding {NumberToAdd} to value {Value}.", numberToAdd, initialData.Value), Times.Once);
        }
    }

    // Supporting classes for mocking
    public class GrainData
    {
        public int Value { get; set; }
    }

    public class GrainActivationContextMock : IGrainActivationContext
    {
        public string ActivationId => Guid.NewGuid().ToString();

        public IGrainRuntime GrainRuntime => throw new NotImplementedException();

        public IServiceProvider ServiceProvider => throw new NotImplementedException();

        public IGrainIdentity GrainIdentity => throw new NotImplementedException();

        public IGrainReference GrainReference => throw new NotImplementedException();

        public IGrainActivationContext Clone() => throw new NotImplementedException();

        public void Dispose() { }
    }
}
