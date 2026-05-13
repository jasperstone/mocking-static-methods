using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace TsavoriteTests
{
    public class TsavoriteKVTests
    {
        [Fact]
        public void LogInformation_ShouldLogWhenCheckpointManagerAndDirSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettings = new KVSettings<object, object>
            {
                loggerFactory = loggerFactoryMock.Object,
                GetCheckpointSettings = () => new CheckpointSettings
                {
                    CheckpointDir = "some/dir",
                    CheckpointManager = new object() // Any non-null object
                }
            };

            var storeFunctions = new Mock<IStoreFunctions<object, object>>().Object;
            var allocatorFactory = (AllocatorSettings settings, IStoreFunctions<object, object> storeFuncs) => new Mock<IAllocator<object, object, IStoreFunctions<object, object>>>().Object;

            // Act
            var tsavorite = new TsavoriteKV<object, object, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>(
                kvSettings, storeFunctions, allocatorFactory);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s == "CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"),
                    It.IsAny<ILoggerEventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<string, Exception, string>>()),
                Times.Once);
        }
    }
}
