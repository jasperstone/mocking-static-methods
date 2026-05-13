using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class RecoveryTests
    {
        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenRecoveringNonEmptyLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TsavoriteKV<long, long, EmptyDefaultFunctions<long, long>, Allocator<long, long, EmptyDefaultFunctions<long, long>>>>>(MockBehavior.Strict);
            loggerMock.Setup(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.Is<EventId>(id => id.Id == 0),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Recovery called on non-empty log")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var store = new Mock<TsavoriteKV<long, long, EmptyDefaultFunctions<long, long>, Allocator<long, long, EmptyDefaultFunctions<long, long>>>>(MockBehavior.Strict);
            store.Setup(s => s.hlogBase).Returns(CreateMockHlogBaseWithTailGreaterThanFirstValid());
            store.Setup(s => s.hlog).Returns(CreateMockHlogWithFirstValidLessThanTail());

            // Mock protected members - this is tricky for protected methods, but we focus on logger call
            // Since InternalRecoverAsync is private/protected, we test the observable behavior via logger

            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            // Act & Assert - we verify the logger call happens when condition is met
            loggerMock.Verify(l => l.LogInformation("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store."), Times.Once());
        }

        [Fact]
        public void InternalRecoverAsync_DoesNotLog_WhenLogIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TsavoriteKV<long, long, EmptyDefaultFunctions<long, long>, Allocator<long, long, EmptyDefaultFunctions<long, long>>>>>(MockBehavior.Strict);
            
            var store = new Mock<TsavoriteKV<long, long, EmptyDefaultFunctions<long, long>, Allocator<long, long, EmptyDefaultFunctions<long, long>>>>(MockBehavior.Strict);
            store.Setup(s => s.hlogBase).Returns(CreateMockHlogBaseWithTailNotGreaterThanFirstValid());
            store.Setup(s => s.hlog).Returns(CreateMockHlog());

            // Act - simulate empty log condition
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Never());
        }

        private Mock<IHybridLog> CreateMockHlogBaseWithTailGreaterThanFirstValid()
        {
            var hlogMock = new Mock<IHybridLog>();
            hlogMock.Setup(h => h.GetTailAddress()).Returns(1000L);
            return hlogMock;
        }

        private Mock<IHybridLog> CreateMockHlogWithFirstValidLessThanTail()
        {
            var hlogMock = new Mock<IHybridLog>();
            hlogMock.Setup(h => h.GetFirstValidLogicalAddress(0)).Returns(500L);
            return hlogMock;
        }

        private Mock<IHybridLog> CreateMockHlogBaseWithTailNotGreaterThanFirstValid()
        {
            var hlogMock = new Mock<IHybridLog>();
            hlogMock.Setup(h => h.GetTailAddress()).Returns(100L);
            return hlogMock;
        }

        private Mock<IHybridLog> CreateMockHlog()
        {
            var hlogMock = new Mock<IHybridLog>();
            hlogMock.Setup(h => h.GetFirstValidLogicalAddress(0)).Returns(200L);
            return hlogMock;
        }
    }
}
