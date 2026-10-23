using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionLoggingTests
    {
        [Fact]
        public void LogError_Should_Be_Called_When_SyncAddress_Less_Than_BeginAddress_And_PossibleAofDataLoss_IsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var syncAddress = 50L;
            var beginAddress = 100L;
            var possibleAofDataLoss = false;

            // Act
            if (syncAddress < beginAddress && !possibleAofDataLoss)
            {
                loggerMock.Object.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", syncAddress, beginAddress);
            }

            // Assert
            loggerMock.Verify(x => x.LogError(It.IsAny<string>(), syncAddress, beginAddress), Times.Once);
        }
    }
}
