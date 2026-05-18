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
        public async Task InternalRecoverAsync_LogsInformation_WhenLogIsNotEmpty()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TsavoriteKV<int, string, DummyStoreFunctions, DummyAllocator>>>();
            var recoveryInstance = new TsavoriteKV<int, string, DummyStoreFunctions, DummyAllocator>();
            recoveryInstance.logger = mockLogger.Object;

            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();

            // Setup the log to simulate non-empty log
            var hlogBaseMock = new Mock<IHlogBase>();
            hlogBaseMock.Setup(h => h.GetTailAddress()).Returns(100);
            recoveryInstance.hlogBase = hlogBaseMock.Object;

            var hlogMock = new Mock<IHlog>();
            recoveryInstance.hlog = hlogMock.Object;

            // Act
            await recoveryInstance.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 10, false, 200, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Recovery called on non-empty log"))),
                Times.Once);
        }
    }

    // Dummy implementations for the generic parameters
    public class DummyStoreFunctions : IStoreFunctions<int, string> { }
    public class DummyAllocator : IAllocator<int, string, DummyStoreFunctions> { }
}
