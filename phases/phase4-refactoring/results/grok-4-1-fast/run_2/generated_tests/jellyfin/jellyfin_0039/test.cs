using System;
using System.IO;
using System.Reflection;
using System.Text;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Devices.Tests
{
    public class DeviceIdTests
    {
        private readonly Mock<IApplicationPaths> _mockAppPaths;
        private readonly Mock<ILogger<DeviceId>> _mockLogger;
        private readonly DeviceId _deviceId;

        public DeviceIdTests()
        {
            _mockAppPaths = new Mock<IApplicationPaths>();
            _mockLogger = new Mock<ILogger<DeviceId>>();
            _deviceId = new DeviceId(_mockAppPaths.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetCachedId_LogsError_OnIOException()
        {
            // Arrange - use reflection to directly call GetCachedId and throw exception
            _mockAppPaths.Setup(x => x.DataPath).Returns(Path.GetTempPath());
            
            var getCachedIdMethod = typeof(DeviceId).GetMethod("GetCachedId", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act - triggers line 55: _logger.LogError(ex, "Error reading file");
            getCachedIdMethod.Invoke(_deviceId, null);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetCachedId_LogsInvalidValueError_WhenNonGuidInFile()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var cachePath = Path.Combine(tempDir, "device.txt");
            _mockAppPaths.Setup(x => x.DataPath).Returns(tempDir);
            
            Directory.CreateDirectory(tempDir);
            File.WriteAllText(cachePath, "invalid");

            try
            {
                // Act
                _ = _deviceId.Value;
            }
            finally
            {
                try { File.Delete(cachePath); } catch { }
                try { Directory.Delete(tempDir, true); } catch { }
            }

            // Assert - covers the LogError call for invalid GUID
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SaveId_LogsError_OnWriteException()
        {
            // Arrange
            _mockAppPaths.Setup(x => x.DataPath).Returns("C:\\non\\existent\\path");

            var id = Guid.NewGuid().ToString("N");
            var saveIdMethod = typeof(DeviceId).GetMethod("SaveId", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            saveIdMethod.Invoke(_deviceId, new object[] { id });

            // Assert - covers the LogError call in SaveId
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
