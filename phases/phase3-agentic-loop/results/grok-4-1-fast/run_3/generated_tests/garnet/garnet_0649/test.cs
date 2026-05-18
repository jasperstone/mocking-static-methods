using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests;

public class RecoveryLoggerTests
{
    [Fact]
    public async Task InternalRecoverAsync_LogsInformationMessage_WhenLogIsNonEmpty()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var mockHlogBase = new Mock<HybridLogBase>();
        var mockHlog = new Mock<HybridLog>();
        
        mockHlogBase.Setup(x => x.GetTailAddress()).Returns(2000L);
        mockHlog.Setup(x => x.GetFirstValidLogicalAddress(0)).Returns(1000L);
        
        // Create real TsavoriteKV instance with mocked dependencies
        var storeFunctions = new DefaultStoreFunctions<long, long>();
        var allocator = new Allocator<long, long, DefaultStoreFunctions<long, long>>(null!, null!, null!);
        var store = new TsavoriteKV<long, long, DefaultStoreFunctions<long, long>, Allocator<long, long, DefaultStoreFunctions<long, long>>>(
            null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!);
        
        // Use reflection to set private fields
        typeof(TsavoriteKV<long, long, DefaultStoreFunctions<long, long>, Allocator<long, long, DefaultStoreFunctions<long, long>>>)
            .GetField("hlogBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(store, mockHlogBase.Object);
        typeof(TsavoriteKV<long, long, DefaultStoreFunctions<long, long>, Allocator<long, long, DefaultStoreFunctions<long, long>>>)
            .GetField("hlog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(store, mockHlog.Object);
        typeof(TsavoriteKV<long, long, DefaultStoreFunctions<long, long>, Allocator<long, long, DefaultStoreFunctions<long, long>>>)
            .GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(store, logger.Object);

        // Mock methods that would otherwise throw
        mockHlogBase.Setup(x => x.VerifyRecoveryInfo(It.IsAny<HybridLogCheckpointInfo>(), false));
        typeof(TsavoriteKV<long, long, DefaultStoreFunctions<long, long>, Allocator<long, long, DefaultStoreFunctions<long, long>>>)
            .GetMethod("RecoverToInitialPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(store, new object[] { new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), null });

        // Act
        var cancellationToken = CancellationToken.None;
        await Assert.ThrowsAsync<NotImplementedException>(() => 
            store.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, cancellationToken));

        // Assert - Verify the LogInformation call happened before the exception
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    ((string)v.ToString()).Contains("Recovery called on non-empty log - resetting to empty state first")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
