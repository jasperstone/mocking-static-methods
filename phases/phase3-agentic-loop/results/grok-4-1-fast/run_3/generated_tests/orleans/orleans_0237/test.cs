using System;
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
        public async Task OnActivateAsync_ShouldLogInformationWithGrainId()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            
            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            
            var grain = new TestableSingleStateFaultInjectionTransactionalGrain(
                dataMock.Object,
                loggerFactoryMock.Object);

            // Act
            await grain.OnActivateAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GrainId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Set_ShouldLogInformationWithNewValue()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            
            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            dataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>())).Returns(Task.CompletedTask);
            
            var grain = new TestableSingleStateFaultInjectionTransactionalGrain(
                dataMock.Object,
                loggerFactoryMock.Object);
            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Set(42);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Setting value")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Add_ShouldLogInformationWithNumberToAddAndCurrentValue()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            
            var dataMock = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            dataMock.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>())).Returns(Task.CompletedTask);
            
            var grain = new TestableSingleStateFaultInjectionTransactionalGrain(
                dataMock.Object,
                loggerFactoryMock.Object);
            await grain.OnActivateAsync(CancellationToken.None);

            // Act
            await grain.Add(5);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    public class TestableSingleStateFaultInjectionTransactionalGrain : SingleStateFaultInjectionTransactionalGrain
    {
        public TestableSingleStateFaultInjectionTransactionalGrain(
            IFaultInjectionTransactionalState<GrainData> data,
            ILoggerFactory loggerFactory) : base(data, loggerFactory)
        {
        }

        public new Task OnActivateAsync(CancellationToken cancellationToken) => 
            base.OnActivateAsync(cancellationToken);

        public new Task Set(int newValue) => base.Set(newValue);
        
        public new Task Add(int numberToAdd, FaultInjectionControl faultInjectionControl = null) => 
            base.Add(numberToAdd, faultInjectionControl);
    }
}
