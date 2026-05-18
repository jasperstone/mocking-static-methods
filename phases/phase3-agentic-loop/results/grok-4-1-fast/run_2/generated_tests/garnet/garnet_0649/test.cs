using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tsavorite.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenRecoveringNonEmptyLog()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHlogBase = new Mock<HybridLog>();
            var mockHlog = new Mock<HybridLog>();
            
            mockHlogBase.Setup(x => x.GetTailAddress()).Returns(1000L);
            mockHlog.Setup(x => x.GetFirstValidLogicalAddress(It.IsAny<int>())).Returns(500L);
            
            var tsavoriteMock = new Mock<TsavoriteKV<long, long, object, object>>();
            tsavoriteMock.SetupProperty(x => x.logger, mockLogger.Object);
            tsavoriteMock.Setup(x => x.hlogBase).Returns(mockHlogBase.Object);
            tsavoriteMock.Setup(x => x.hlog).Returns(mockHlog.Object);
            
            // Mock other methods to prevent exceptions and allow execution to reach the log call
            tsavoriteMock.Setup(x => x.Reset()).Returns();
            tsavoriteMock.Setup(x => x.RecoverToInitialPage(It.IsAny<IndexCheckpointInfo>(), It.IsAny<HybridLogCheckpointInfo>(), out It.Ref<long>.IsAny))
                        .Returns(true);
            tsavoriteMock.Setup(x => x.SetRecoveryPageRanges(It.IsAny<HybridLogCheckpointInfo>(), It.IsAny<int>(), It.IsAny<long>(), 
                        out It.Ref<long>.IsAny, out It.Ref<long>.IsAny, out It.Ref<long>.IsAny))
                        .Returns(true);
            
            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            
            // Act
            try
            {
                await tsavoriteMock.Object.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 0, false, 0, CancellationToken.None);
            }
            catch
            {
                // Ignore exceptions from unmocked methods
            }
            
            // Assert - verify LogInformation was called with the specific message from line ~500
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store.") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        
        [Fact]
        public void GetLatestCheckpointTokens_LogsInformation_WhenNoIndexCheckpointFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            
            var tsavoriteMock = new Mock<TsavoriteKV<long, long, object, object>>();
            tsavoriteMock.SetupProperty(x => x.logger, mockLogger.Object);
            
            // Mock GetClosestHybridLogCheckpointInfo to return a valid token
            Guid hlogToken = Guid.NewGuid();
            var hlcInfo = new HybridLogCheckpointInfo();
            tsavoriteMock.Setup(x => x.GetClosestHybridLogCheckpointInfo(-1, out It.Ref<Guid>.IsAny, out It.Ref<HybridLogCheckpointInfo>.IsAny, out It.Ref<LogFileInfo>.IsAny))
                        .Callback<long, Guid, HybridLogCheckpointInfo, LogFileInfo>((addr, refToken, refHlc, refLfi) => {
                            refToken = hlogToken;
                            refHlc = hlcInfo;
                        });
            
            // Mock GetClosestIndexCheckpointInfo to return default (IsDefault() == true)
            tsavoriteMock.Setup(x => x.GetClosestIndexCheckpointInfo(It.IsAny<HybridLogCheckpointInfo>(), out It.Ref<Guid>.IsAny, out It.Ref<IndexCheckpointInfo>.IsAny))
                        .Callback<HybridLogCheckpointInfo, Guid, IndexCheckpointInfo>((hlc, refToken, refIcInfo) => {
                            refToken = Guid.Empty;
                            refIcInfo = default; // triggers IsDefault()
                        });
            
            // Act
            tsavoriteMock.Object.GetLatestCheckpointTokens(out _, out _, out _);
            
            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("No index checkpoint found, returning default index token in GetLatestCheckpointTokens") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
