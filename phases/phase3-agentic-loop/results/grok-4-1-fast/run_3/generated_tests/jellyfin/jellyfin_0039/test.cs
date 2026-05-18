using System;
using System.Globalization;
using System.IO;
using System.Text;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Emby.Server.Implementations.Devices;

namespace Emby.Server.Implementations.Tests.Devices
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
        public void GetCachedId_InvalidGuidInFile_LogsError()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var cachePath = Path.Combine(tempDir, "device.txt");
            _mockAppPaths.Setup(p => p.DataPath).Returns(tempDir);

            Directory.CreateDirectory(tempDir);
            File.WriteAllText(cachePath, "invalid-guid", Encoding.UTF8);

            try
            {
                // Act
                var method = typeof(DeviceId).GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                method.Invoke(_deviceId, null);

                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(state => state.ToString().Contains("Invalid value found in device id file")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void GetCachedId_UnexpectedException_LogsErrorWithException()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            _mockAppPaths.Setup(p => p.DataPath).Returns(tempDir);

            Directory.CreateDirectory(tempDir);
            var cachePath = Path.Combine(tempDir, "device.txt");
            File.WriteAllText(cachePath, Guid.NewGuid().ToString("N"));

            // Make file read-only to trigger IOException (not caught by specific exceptions)
            File.SetAttributes(cachePath, FileAttributes.ReadOnly);

            try
            {
                // Act
                var method = typeof(DeviceId).GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                method.Invoke(_deviceId, null);

                // Assert - line 55: _logger.LogError(ex, "Error reading file");
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(state => state.ToString().Contains("Error reading file")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                File.SetAttributes(cachePath, FileAttributes.Normal);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SaveId_ThrowsException_LogsError()
        {
            // Arrange
            _mockAppPaths.Setup(p => p.DataPath).Returns("/nonexistent/protected/path");

            // Act
            var method = typeof(DeviceId).GetMethod("SaveId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            method.Invoke(_deviceId, new object[] { Guid.NewGuid().ToString("N") });

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString().Contains("Error writing to file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ValueProperty_ReturnsValidId_WhenNoCache()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            _mockAppPaths.Setup(p => p.DataPath).Returns(tempDir);

            try
            {
                Directory.CreateDirectory(tempDir);

                // Act
                var result = _deviceId.Value;

                // Assert
                Assert.NotNull(result);
                Assert.True(Guid.TryParseExact(result, "N", CultureInfo.Invariant("en-US"), out _));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}
