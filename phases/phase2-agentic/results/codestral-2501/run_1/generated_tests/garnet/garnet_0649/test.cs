using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class RecoveryTests
    {
        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenLogIsNotEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recovery = new TsavoriteKV<string, string, MockStoreFunctions, MockAllocator>
            {
                logger = loggerMock.Object
            };

            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo
            {
                info = new HybridLogCheckpointInfo.CheckpointInfo
                {
                    nextVersion = 1
                }
            };

            var hlogBaseMock = new Mock<HybridLogBase>();
            hlogBaseMock.Setup(h => h.GetTailAddress()).Returns(100);
            hlogBaseMock.Setup(h => h.GetFirstValidLogicalAddress(0)).Returns(50);
            recovery.hlogBase = hlogBaseMock.Object;

            // Act
            await recovery.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 0, false, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }

    public class MockStoreFunctions : IStoreFunctions<string, string>
    {
        public void Dispose() { }
        public void Initialize() { }
        public void Reset() { }
        public void Shutdown() { }
        public void Update(string key, string value) { }
        public void Delete(string key) { }
        public string Get(string key) => null;
    }

    public class MockAllocator : IAllocator<string, string, MockStoreFunctions>
    {
        public void Dispose() { }
        public void Initialize() { }
        public void Reset() { }
        public void Shutdown() { }
        public void Update(string key, string value) { }
        public void Delete(string key) { }
        public string Get(string key) => null;
    }
}
