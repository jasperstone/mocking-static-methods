using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
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

            // Simulate a non-empty log condition
            recovery.hlogBase = new Mock<HybridLogBase>().Object;
            recovery.hlogBase.GetTailAddress().Returns(100L); // Non-zero tail address
            recovery.hlogBase.GetFirstValidLogicalAddress(0).Returns(0L); // Zero first valid address

            // Act
            await recovery.InternalRecoverAsync(
                new IndexCheckpointInfo(),
                new HybridLogCheckpointInfo(),
                0,
                false,
                0,
                CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("Recovery called on non-empty log")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
