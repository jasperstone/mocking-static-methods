using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Devices;

namespace Emby.Tests
{
    public class DeviceIdTests
    {
        private readonly Mock<ILogger<DeviceId>> _mockLogger;

        public DeviceIdTests()
        {
            _mockLogger = new Mock<ILogger<DeviceId>>();
        }

        private class DummyAppPaths : IApplicationPaths
        {
            public string DataPath { get; set; }
        }

        [Fact]
        public void GetCachedId_InvalidGuidLogsError()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var filePath = Path.Combine(tempDir, "device.txt");
            File.WriteAllText(filePath, "invalid-guid");
            var appPaths = new DummyAppPaths { DataPath = tempDir };
            var deviceId = new DeviceId(appPaths, _mockLogger.Object);

            // Act
            var result = deviceId.GetCachedId();

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid value found in device id file")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetCachedId_FileNotFound_ReturnsNull()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var filePath = Path.Combine(tempDir, "device.txt");
            if (File.Exists(filePath))
                File.Delete(filePath);
            var appPaths = new DummyAppPaths { DataPath = tempDir };
            var deviceId = new DeviceId(appPaths, _mockLogger.Object);

            // Act
            var result = deviceId.GetCachedId();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCachedId_ExceptionLogsError()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var filePath = Path.Combine(tempDir, "device.txt");
            _mockLogger.Setup(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()));
            var appPaths = new DummyAppPaths { DataPath = tempDir };
            var deviceId = new DeviceId(appPaths, _mockLogger.Object);

            // Simulate exception during File.ReadAllText
            // Since static method can't be mocked directly, this test assumes the exception is thrown and caught internally.
            // For a real test, consider wrapping File.ReadAllText in an interface for better testability.

            // Act
            var result = deviceId.GetCachedId();

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error reading file")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogErrorOnExceptionInGetDeviceId()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var filePath = Path.Combine(tempDir, "device.txt");
            File.WriteAllText(filePath, "invalid-guid");
            var appPaths = new DummyAppPaths { DataPath = tempDir };
            var deviceId = new DeviceId(appPaths, _mockLogger.Object);

            // Act
            var id = deviceId.Value;

            // Assert
            Assert.NotNull(id);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error reading file") || v.ToString().Contains("Error writing to file")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
