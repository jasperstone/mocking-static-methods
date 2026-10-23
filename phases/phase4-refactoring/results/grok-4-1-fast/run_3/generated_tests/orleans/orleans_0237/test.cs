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
        private readonly Mock<IFaultInjectionTransactionalState<GrainData>> _mockData;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<ILogger> _mockLogger;
        private readonly SingleStateFaultInjectionTransactionalGrain _grain;

        public SingleStateFaultInjectionTransactionalGrainTests()
        {
            _mockData = new Mock<IFaultInjectionTransactionalState<GrainData>>();
            _mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                     .Returns(Task.CompletedTask);

            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLogger = new Mock<ILogger>();

            _mockLoggerFactory
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(_mockLogger.Object);

            _grain = new SingleStateFaultInjectionTransactionalGrain(_mockData.Object, _mockLoggerFactory.Object);
        }

        [Fact]
        public async Task OnActivateAsync_CallsLogInformationWithGrainId()
        {
            // Act
            await _grain.OnActivateAsync(CancellationToken.None);

            // Assert
            _mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
            _mockLogger.VerifyLogInformation("GrainId {GrainId}", Times.Once(), It.IsAny<object[]>());
        }

        [Fact]
        public async Task Set_LogsInformationWithNewValue()
        {
            // Arrange
            const int newValue = 42;

            // Act
            await _grain.Set(newValue);

            // Assert
            _mockLogger.VerifyLogInformation("Setting value {NewValue}.", Times.Once(), newValue);
        }

        [Fact]
        public async Task Add_LogsInformationWithNumberToAddAndCurrentValue()
        {
            // Arrange
            const int numberToAdd = 10;
            var grainData = new GrainData { Value = 5 };

            _mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                     .Callback<Action<GrainData>>(action => action(grainData))
                     .Returns(Task.CompletedTask);

            // Act
            await _grain.Add(numberToAdd);

            // Assert
            _mockLogger.VerifyLogInformation("Adding {NumberToAdd} to value {Value}.", Times.Once(), numberToAdd, 5);
        }
    }

    public class GrainData
    {
        public int Value { get; set; }
    }
}
