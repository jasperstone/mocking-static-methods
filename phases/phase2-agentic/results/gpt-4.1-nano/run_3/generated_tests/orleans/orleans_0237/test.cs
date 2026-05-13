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
using Orleans.Core;
using Orleans.TestingHost.Utils;

namespace Orleans.Transactions.Test
{
    public class SingleStateDeactivatingTransactionalGrainTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IGrainIdentity> _grainIdentityMock;
        private readonly Mock<IGrainRuntime> _grainRuntimeMock;
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> _dataMock;

        public SingleStateDeactivatingTransactionalGrainTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

            _grainIdentityMock = new Mock<IGrainIdentity>();
            _grainRuntimeMock = new Mock<IGrainRuntime>();
            _dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
        }

        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainId()
        {
            // Arrange
            var grain = new SingleStateFaultInjectionTransactionalGrain(_dataMock.Object, _loggerFactoryMock.Object);
            var grainId = Guid.NewGuid();
            var grainIdMock = new Mock<IGrainIdentity>();
            grainIdMock.Setup(g => g.ToString()).Returns(grainId.ToString());
            var grainRuntimeMock = new Mock<IGrainRuntime>();
            grainRuntimeMock.Setup(r => r.GetGrainId()).Returns(grainIdMock.Object);
            var grainRef = new Mock<IGrain>();
            var grainClass = typeof(SingleStateFaultInjectionTransactionalGrain);
            var grainInstance = (SingleStateFaultInjectionTransactionalGrain)Activator.CreateInstance(grainClass, _dataMock.Object, _loggerFactoryMock.Object);
            // Manually set the logger
            grainInstance.GetType().GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(grainInstance, _loggerMock.Object);
            // Act
            await grainInstance.OnActivateAsync(CancellationToken.None);
            // Assert
            _loggerMock.Verify(l => l.LogInformation("GrainId {GrainId}", It.IsAny<object>()), Times.Once);
        }
    }
}
