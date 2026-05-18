using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Base.Tests
{
    public class SingleStateFaultInjectionTransactionalGrainTests
    {
        [Fact]
        public async Task OnActivateAsync_ShouldLogGrainIdInformation()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var dataMock = Mock.Of<IFaultInjectionTransactionalState<GrainData>>();

            // Create test subclass to avoid GrainId null reference
            var grain = new TestGrain(dataMock, loggerFactoryMock.Object);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            loggerFactoryMock.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
            loggerMock.Verify(l => l.LogInformation("GrainId {GrainId}", It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogSettingValueInformation()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            // Mock the actual interface method instead of extension
            dataMock.Setup(d => d.UpdateAsync(It.IsAny<Func<GrainData, Task>>(), It.IsAny<string>()))
                   .Returns(Task.CompletedTask);
            
            var grain = new TestGrain(dataMock.Object, loggerFactoryMock.Object);

            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Set(42);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Setting value {NewValue}.", 42), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldLogAddingValueInformation()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            dataMock.Setup(d => d.UpdateAsync(It.IsAny<Func<GrainData, Task>>(), It.IsAny<string>()))
                   .Returns(Task.CompletedTask);
            
            var grain = new TestGrain(dataMock.Object, loggerFactoryMock.Object);

            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Add(10);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Adding")), It.IsAny<object[]>()), Times.Once);
        }

        private class TestGrain : SingleStateFaultInjectionTransactionalGrain
        {
            public TestGrain(IFaultInjectionTransactionalState<GrainData> data, ILoggerFactory loggerFactory)
                : base(data, loggerFactory)
            {
            }

            public override GrainId GetGrainId() => GrainId.Create("test-type", "test-key");
            public override string GetPrimaryKey() => "test-key";
        }
    }
}
