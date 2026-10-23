using System;
using System.IO;
using Emby.Server.Implementations.Devices;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
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
            _mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
            _deviceId = new DeviceId(_mockAppPaths.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetCachedId_WhenFileReadThrowsException_LogsErrorReadingFile()
        {
            // Arrange
            _mockAppPaths.Setup(p => p.DataPath).Returns("/nonexistent/path");

            // Act
            _deviceId.Value;

            // Assert - verifies the LogError(ex, "Error reading file") call on line 55
            _mockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), 
                    It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Error reading file") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SaveId_WhenWriteThrowsException_LogsErrorWritingToFile()
        {
            // Arrange
            _mockAppPaths.Setup(p => p.DataPath).Returns("/protected/path");

            // Act - triggers new ID generation and SaveId
            _deviceId.Value;

            // Assert
            _mockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), 
                    It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Error writing to file") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void GetCachedId_WhenCacheContainsInvalidGuid_LogsInvalidValueFound()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var cacheFile = Path.Combine(tempDir, "device.txt");
            Directory.CreateDirectory(tempDir);
            File.WriteAllText(cacheFile, "invalid-guid");

            _mockAppPaths.Setup(p => p.DataPath).Returns(tempDir);

            try
            {
                // Act
                _deviceId.Value;

                // Assert
                _mockLogger.Verify(
                    x => x.Log(LogLevel.Error, It.IsAny<EventId>(), 
                        It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Invalid value found in device id file") == true),
                        It.IsNull<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                try { if (File.Exists(cacheFile)) File.Delete(cacheFile); } catch { }
                try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }
            }
        }

        [Fact]
        public void Value_ReturnsValidGuid_WhenNoCacheFileExists()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            _mockAppPaths.Setup(p => p.DataPath).Returns(tempDir);

            try
            {
                // Act
                var id1 = _deviceId.Value;
                var id2 = _deviceId.Value;

                // Assert
                Assert.Equal(id1, id2);
                Assert.True(Guid.TryParseExact(id1, "N", null, out _));
            }
            finally
            {
                try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }
            }
        }
    }
}
