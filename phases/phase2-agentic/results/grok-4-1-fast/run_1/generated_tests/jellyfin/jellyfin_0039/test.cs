using System;
using System.IO;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
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
        public void GetCachedId_ThrowsGeneralException_LogsError()
        {
            // Arrange
            _mockAppPaths.Setup(p => p.DataPath).Returns("/test/data");
            var mockException = new UnauthorizedAccessException("Access denied");

            // Act & Assert
            var result = _deviceId.GetCachedId(); // Internal call via Value, but we test the logging path

            // Since GetCachedId is private, we trigger through public Value and verify logger was called
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t.ToString().Contains("Error reading file")),
                    mockException,
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Value_GetDeviceIdTriggersReadError_LogsErrorAndGeneratesNewId()
        {
            // Arrange
            _mockAppPaths.Setup(p => p.DataPath).Returns("/test/data");

            // Act
            var result = _deviceId.Value;

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t.ToString().Contains("Error reading file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void SaveId_ThrowsException_LogsError()
        {
            // Arrange
            _mockAppPaths.Setup(p => p.DataPath).Returns("/test/data");
            var mockException = new IOException("Disk full");

            // Use reflection to call private SaveId method
            var saveIdMethod = typeof(DeviceId).GetMethod("SaveId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            saveIdMethod!.Invoke(_deviceId, new object[] { "test-id" });

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t.ToString().Contains("Error writing to file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetCachedId_InvalidGuidFormat_LogsInvalidValueError()
        {
            // Arrange
            _mockAppPaths.Setup(p => p.DataPath).Returns(Path.Combine(Path.GetTempPath(), "test-data"));
            var cachePath = Path.Combine(_mockAppPaths.Object.DataPath!, "device.txt");
            
            // Create invalid content
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            File.WriteAllText(cachePath, "invalid-guid", System.Text.Encoding.UTF8);

            try
            {
                // Act
                _deviceId.GetCachedId();

                // Assert
                _mockLogger.Verify(
                    logger => logger.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyFormat<string>>((v, t) => t.ToString().Contains("Invalid value found in device id file")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                    Times.Once);
            }
            finally
            {
                // Cleanup
                if (File.Exists(cachePath))
                    File.Delete(cachePath);
                if (Directory.Exists(Path.GetDirectoryName(cachePath)!))
                    Directory.Delete(Path.GetDirectoryName(cachePath)!, true);
            }
        }
    }
}
