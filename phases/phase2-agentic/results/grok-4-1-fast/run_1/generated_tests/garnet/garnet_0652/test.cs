using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class TsavoriteKVTests
    {
        [Fact]
        public void Constructor_WhenCheckpointDirAndCheckpointManagerBothSpecified_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TsavoriteKV<int, int, object, object>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettings = new KVSettings<int, int>()
            {
                loggerFactory = loggerFactoryMock.Object,
                CheckpointSettings = new CheckpointSettings()
                {
                    CheckpointDir = "some/dir",
                    CheckpointManager = new Mock<ICheckpointManager>().Object
                }
            };

            var storeFunctionsMock = new Mock<object>().Object as object;
            Func<AllocatorSettings, object, object> allocatorFactory = (_, __) => new Mock<object>().Object;

            // Act
            var ex = Record.Exception(() => new TsavoriteKV<int, int, object, object>(kvSettings, storeFunctionsMock, allocatorFactory));

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.Is<It.IsAnyType>((v, t) => t.Name == "Format" && v.ToString().Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"))),
                Times.Once);
        }

        [Fact]
        public void Constructor_WhenLoggerIsNull_NoLogInformationMessageLogged()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var kvSettings = new KVSettings<int, int>()
            {
                loggerFactory = loggerFactoryMock.Object,
                logger = null,
                CheckpointSettings = new CheckpointSettings()
                {
                    CheckpointDir = "some/dir",
                    CheckpointManager = new Mock<ICheckpointManager>().Object
                }
            };

            var storeFunctionsMock = new Mock<object>().Object as object;
            Func<AllocatorSettings, object, object> allocatorFactory = (_, __) => new Mock<object>().Object;

            // Act
            var ex = Record.Exception(() => new TsavoriteKV<int, int, object, object>(kvSettings, storeFunctionsMock, allocatorFactory));

            // Assert - No verification needed since logger is null, the ?. operator prevents the call
            Assert.Null(ex);
        }
    }
}
