using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class RecoveryTests
    {
        [Fact]
        public async Task LogInformation_ShouldBeCalled_WhenRecoveryCalledOnNonEmptyLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recovery = new Recovery(loggerMock.Object);

            // Simulate a non-empty log
            recovery.hlogBase = new Mock<HybridLogBase>().Object;
            recovery.hlogBase.Setup(h => h.GetTailAddress()).Returns(1L);
            recovery.hlogBase.Setup(h => h.GetFirstValidLogicalAddress(It.IsAny<int>())).Returns(0L);

            // Act
            await recovery.InternalRecoverAsync(default, default, 0, false, 0, default);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("Recovery called on non-empty log")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
