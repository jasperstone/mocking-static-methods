using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class RecoveryLoggingTests
    {
        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenLogIsNonEmpty()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Recovery>>();
            var recovery = new Recovery
            {
                logger = mockLogger.Object,
                hlogBase = new Mock<IHlogBase>().Object,
                hlog = new Mock<IHlog>().Object
            };

            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            var cancellationToken = new CancellationToken();

            // Setup the recovery info to simulate non-empty log
            var tailAddress = 100;
            var firstValidLogicalAddress = 50;
            var getTailAddressCalled = false;
            var getFirstValidLogicalAddressCalled = false;

            var mockHlogBase = new Mock<IHlogBase>();
            mockHlogBase.Setup(h => h.GetTailAddress()).Returns(() =>
            {
                getTailAddressCalled = true;
                return tailAddress;
            });
            mockHlogBase.Setup(h => h.GetFirstValidLogicalAddress(0)).Returns(() =>
            {
                getFirstValidLogicalAddressCalled = true;
                return firstValidLogicalAddress;
            });
            recovery.hlogBase = mockHlogBase.Object;

            var mockHlog = new Mock<IHlog>();
            recovery.hlog = mockHlog.Object;

            // Act
            await recovery.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 10, false, 200, cancellationToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Recovery called on non-empty log")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
