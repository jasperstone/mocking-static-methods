using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class TsavoriteKVTests
    {
        private class DummyAllocator : IAllocator<int, int, object>
        {
            public bool IsFixedLength => true;
            public bool HasObjectLog => false;
            public AllocatorBase<int, int, object, DummyAllocator> GetBase<T>() where T : IAllocator<int, int, object> => new DummyAllocatorBase();
        }
        private class DummyAllocatorBase : AllocatorBase<int, int, object, DummyAllocator>
        {
            public override void Initialize() { }
            public override long MaxAllocatedPageCount => 0;
        }

        [Fact]
        public void Constructor_LogsInformation_WhenCheckpointManagerAndDirSpecified()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockCheckpointManager = new Mock<object>();

            var mockKVSettings = new Mock<KVSettings<int, int>>();
            mockKVSettings.SetupGet(s => s.logger).Returns(mockLogger.Object);
            mockKVSettings.SetupGet(s => s.loggerFactory).Returns(mockLoggerFactory.Object);
            mockKVSettings.Setup(s => s.GetCheckpointSettings()).Returns(new CheckpointSettings
            {
                CheckpointDir = "someDir",
                CheckpointManager = mockCheckpointManager.Object
            });
            mockKVSettings.Setup(s => s.GetLogSettings()).Returns(new LogSettings
            {
                ReadCopyOptions = new ReadCopyOptions(),
                ReadCacheSettings = new ReadCacheSettings()
            });
            mockKVSettings.SetupGet(s => s.ReadCacheEnabled).Returns(false);
            mockKVSettings.SetupGet(s => s.Epoch).Returns("epoch");

            var mockStoreFunctions = new Mock<object>();

            Func<AllocatorSettings, object, DummyAllocator> allocatorFactory = (settings, sf) => new DummyAllocator();

            // Act
            var tsavorite = new TsavoriteKV<int, int, object, DummyAllocator>(mockKVSettings.Object, mockStoreFunctions.Object, allocatorFactory);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
