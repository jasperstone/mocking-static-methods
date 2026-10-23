using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class TsavoriteKVLoggerTests
    {
        [Fact]
        public void Constructor_WhenBothCheckpointDirAndCheckpointManagerSpecified_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )).Verifiable();

            var kvSettingsMock = new Mock<KVSettings<int, int>>();
            kvSettingsMock.Setup(s => s.GetCheckpointSettings()).Returns(new object());
            kvSettingsMock.Setup(s => s.logger).Returns(loggerMock.Object);
            kvSettingsMock.Setup(s => s.loggerFactory).Returns((ILoggerFactory)null);
            kvSettingsMock.Setup(s => s.ReadCacheEnabled).Returns(false);
            kvSettingsMock.Setup(s => s.GetLogSettings()).Returns(new object());

            var storeFunctionsMock = new Mock<IStoreFunctions<int, int>>();
            Func<AllocatorSettings, IStoreFunctions<int, int>, object> allocatorFactory = 
                (_, __) => new object();

            // Act
            var exception = Record.Exception(() => 
                new TsavoriteKV<int, int, IStoreFunctions<int, int>, object>(
                    kvSettingsMock.Object, storeFunctionsMock.Object, allocatorFactory));

            // Assert
            Assert.Null(exception);
            loggerMock.Verify();
        }

        [Fact]
        public void Constructor_WhenCheckpointDirNull_LogsNoInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var kvSettingsMock = new Mock<KVSettings<int, int>>();
            kvSettingsMock.Setup(s => s.GetCheckpointSettings()).Returns(new object() { });
            kvSettingsMock.Setup(s => s.logger).Returns(loggerMock.Object);
            kvSettingsMock.Setup(s => s.ReadCacheEnabled).Returns(false);
            kvSettingsMock.Setup(s => s.GetLogSettings()).Returns(new object());

            var storeFunctionsMock = new Mock<IStoreFunctions<int, int>>();
            Func<AllocatorSettings, IStoreFunctions<int, int>, object> allocatorFactory = 
                (_, __) => new object();

            // Act
            var exception = Record.Exception(() => 
                new TsavoriteKV<int, int, IStoreFunctions<int, int>, object>(
                    kvSettingsMock.Object, storeFunctionsMock.Object, allocatorFactory));

            // Assert
            Assert.Null(exception);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }
    }
}
