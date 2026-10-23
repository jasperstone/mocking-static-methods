using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class TsavoriteKVLoggerTests
    {
        [Fact]
        public void Constructor_WhenCheckpointDirAndCheckpointManagerBothSpecified_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettings = new KVSettings<int, int>()
            {
                LoggerFactory = loggerFactoryMock.Object,
                CheckpointSettings = new CheckpointSettings()
                {
                    CheckpointDir = "some/dir",
                    CheckpointManager = Mock.Of<ICheckpointManager>()
                }
            };

            // Use mocks that don't require complex generic constraints
            var storeFunctionsMock = new Mock<IStoreFunctions<int, int>>();
            Func<AllocatorSettings, IStoreFunctions<int, int>, object> allocatorFactory = 
                (_, __) => new Mock<IAllocator<int, int, IStoreFunctions<int, int>>>().Object;

            // Act
            var exception = Record.Exception(() => 
                new TsavoriteKV<int, int, IStoreFunctions<int, int>, object>(kvSettings, storeFunctionsMock.Object, allocatorFactory));

            // Assert - verify the LogInformation call was made
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v.ToString()).Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_WhenNeitherCheckpointDirNorCheckpointManagerSpecified_DoesNotLogInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettings = new KVSettings<int, int>()
            {
                LoggerFactory = loggerFactoryMock.Object,
                CheckpointSettings = new CheckpointSettings()
            };

            var storeFunctionsMock = new Mock<IStoreFunctions<int, int>>();
            Func<AllocatorSettings, IStoreFunctions<int, int>, object> allocatorFactory = 
                (_, __) => new Mock<IAllocator<int, int, IStoreFunctions<int, int>>>().Object;

            // Act
            var exception = Record.Exception(() => 
                new TsavoriteKV<int, int, IStoreFunctions<int, int>, object>(kvSettings, storeFunctionsMock.Object, allocatorFactory));

            // Assert - verify no LogInformation call was made
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
