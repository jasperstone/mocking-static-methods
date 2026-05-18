using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class RecoveryLoggerTests
    {
        [Fact]
        public async Task InternalRecoverAsync_LogsInformationWhenRecoveringOnNonEmptyLog()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var tsavorite = new RecoveryTestTsavorite(loggerFactoryMock.Object);
            
            // Setup conditions to hit the LogInformation call (line ~500)
            tsavorite.hlogBaseMock.Setup(x => x.GetTailAddress()).Returns(1000L);
            tsavorite.hlogMock.Setup(x => x.GetFirstValidLogicalAddress(0)).Returns(500L);

            // Act
            await tsavorite.InternalRecoverAsyncTest(default, default, 0, false, 0, CancellationToken.None);

            // Assert - verify the specific LogInformation call on line 500
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString() != null && 
                        v.ToString().Contains("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetLatestCheckpointTokens_LogsNoIndexCheckpointFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var tsavorite = new RecoveryTestTsavorite(loggerFactoryMock.Object);
            tsavorite.SetupGetClosestCheckpoints();

            // Act
            tsavorite.GetLatestCheckpointTokensTest(out _, out _, out _);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString() != null && 
                        v.ToString().Contains("No index checkpoint found, returning default index token in GetLatestCheckpointTokens")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal test implementation to access protected/internal members
    internal class RecoveryTestTsavorite : TsavoriteKV<Empty, Empty, EmptyStoreFunctions, BlittableAllocator<Empty, Empty, EmptyStoreFunctions>>
    {
        public readonly Mock<IHybridLog> hlogBaseMock = new();
        public readonly Mock<IHybridLog> hlogMock = new();

        public RecoveryTestTsavorite(ILoggerFactory loggerFactory)
        {
            loggerFactory_ = loggerFactory;
        }

        public async Task InternalRecoverAsyncTest(object recoveredICInfo, object recoveredHLCInfo, 
            int numPagesToPreload, bool undoNextVersion, long recoverTo, CancellationToken cancellationToken)
        {
            await InternalRecoverAsync((dynamic)recoveredICInfo, (dynamic)recoveredHLCInfo, numPagesToPreload, undoNextVersion, recoverTo, cancellationToken);
        }

        public void GetLatestCheckpointTokensTest(out Guid hlogToken, out Guid indexToken, out long storeVersion)
        {
            GetLatestCheckpointTokens(out hlogToken, out indexToken, out storeVersion);
        }

        public void SetupGetClosestCheckpoints()
        {
            hlogTokenOut = Guid.NewGuid();
            // Default recoveredICInfo will trigger the log message
        }

        protected override void GetClosestHybridLogCheckpointInfo(long untilAddress, out Guid token, out HybridLogCheckpointInfo recoveredHlcInfo, out long closestAddress)
        {
            token = hlogTokenOut;
            recoveredHlcInfo = default;
            closestAddress = 0;
        }

        protected override void GetClosestIndexCheckpointInfo(ref HybridLogCheckpointInfo recoveredHlcInfo, out Guid token, out IndexCheckpointInfo recoveredICInfo)
        {
            token = default;
            recoveredICInfo = default;
        }

        protected override IHybridLog CreateHybridLogInstance(TsavoriteLogSettings logSettings)
        {
            hlogBase = hlogBaseMock.Object;
            hlog = hlogMock.Object;
            return hlogBase;
        }
    }
}
