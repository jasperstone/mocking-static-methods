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
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockLogger = new Mock<ILogger>();

            _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
            
            // Setup PerformUpdate to return Task (void version)
            _mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>())).Returns(Task.CompletedTask);
            _mockData.Setup(d => d.PerformRead<int>(It.IsAny<Func<GrainData, int>>())).ReturnsAsync(0);

            _grain = new SingleStateFaultInjectionTransactionalGrain(_mockData.Object, _mockLoggerFactory.Object);
        }

        [Fact]
        public async Task OnActivateAsync_Calls_LogInformation_WithGrainId()
        {
            // Act
            await _grain.OnActivateAsync(CancellationToken.None);

            // Assert - verifies line 41 logger.LogInformation call
            _mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
            _mockLogger.Verify(l => l.LogInformation("GrainId {GrainId}", It.Is<object[]>(args => args.Length == 1)), Times.Once);
        }

        [Fact]
        public async Task Set_Calls_LogInformation_WithNewValue()
        {
            // Arrange
            int newValue = 42;

            // Act
            await _grain.Set(newValue);

            // Assert
            _mockLogger.Verify(l => l.LogInformation("Setting value {NewValue}.", It.Is<object[]>(args => args.Length == 1 && (int)args[0] == newValue)), Times.Once);
        }

        [Fact]
        public async Task Add_Calls_LogInformation_WithNumberToAddAndCurrentValue()
        {
            // Arrange
            int numberToAdd = 10;
            var mockGrainData = new Mock<GrainData>();
            mockGrainData.SetupGet(d => d.Value).Returns(5);
            
            _mockData.Setup(d => d.PerformUpdate(It.IsAny<Action<GrainData>>()))
                .Callback<Action<GrainData>>(action => action(mockGrainData.Object))
                .Returns(Task.CompletedTask);

            // Act
            await _grain.Add(numberToAdd);

            // Assert
            _mockLogger.Verify(l => l.LogInformation(
                "Adding {NumberToAdd} to value {Value}.",
                It.Is<object[]>(args => 
                    args.Length == 2 && 
                    (int)args[0] == numberToAdd && 
                    (int)args[1] == 5)), 
                Times.Once);
        }
    }
}
