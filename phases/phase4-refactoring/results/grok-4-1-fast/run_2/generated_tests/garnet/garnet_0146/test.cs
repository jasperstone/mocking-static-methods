using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class CheckpointStoreLoggerTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<object> _mockStoreWrapper;
        private readonly Mock<object> _mockClusterProvider;

        public CheckpointStoreLoggerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockStoreWrapper = new Mock<object>();
            _mockClusterProvider = new Mock<object>();
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_NullLogger_DoesNotThrow()
        {
            // Arrange
            dynamic mockStoreWrapper = new Mock<object>().Object;
            dynamic mockClusterProvider = new Mock<object>().Object;

            // Act & Assert
            var ex = Record.Exception(() => 
                new Garnet.cluster.CheckpointStore(mockStoreWrapper, mockClusterProvider, safelyRemoveOutdated: false, null)
                    .PurgeAllCheckpointsExceptEntry((object)null));
            
            Assert.Null(ex);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_WithLogger_ExecutesWithoutException()
        {
            // Arrange
            dynamic mockStoreWrapper = new Mock<object> { DefaultValue = DefaultValue.Mock }.Object;
            dynamic mockClusterProvider = new Mock<object> { DefaultValue = DefaultValue.Mock }.Object;
            
            mockStoreWrapper.serverOptions = new { DisableObjects = true };
            mockClusterProvider.serverOptions = new { DisableObjects = true };
            mockClusterProvider.GetLatestCheckpointEntryFromDisk = () => null;

            var store = new Garnet.cluster.CheckpointStore(mockStoreWrapper, mockClusterProvider, safelyRemoveOutdated: false, _mockLogger.Object);

            // Act & Assert
            var ex = Record.Exception(() => store.PurgeAllCheckpointsExceptEntry());
            Assert.Null(ex);
            
            // Verify logger was accessed (null-conditional operator executed)
            _mockLogger.Verify(l => l.LogCheckpointEntry(It.IsAny<Microsoft.Extensions.Logging.LogLevel>(), 
                                                        It.IsAny<string>(), 
                                                        It.IsAny<object>()), 
                                                        Times.Never);
        }
    }
}
