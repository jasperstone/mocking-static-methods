using System;
using System.Globalization;
using System.IO;
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
            _deviceId = new DeviceId(_mockAppPaths.Object, _mockLogger.Object);
        }

        [Fact]
        public void Value_ReturnsCachedId_WhenValidGuidExists()
        {
            // Arrange
            var testDataPath = Path.Combine(Path.GetTempPath(), "jellyfin-test-data");
            var testCachePath = Path.Combine(testDataPath, "device.txt");
            _mockAppPaths.Setup(x => x.DataPath).Returns(testDataPath);
            
            Directory.CreateDirectory(testDataPath);
            var validGuid = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            File.WriteAllText(testCachePath, validGuid);

            // Act
            var result = _deviceId.Value;

            // Assert
            Assert.Equal(validGuid, result);
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }

        [Fact]
        public void Value_LogsInvalidValueError_WhenCacheContainsInvalidGuid()
        {
            // Arrange
            var testDataPath = Path.Combine(Path.GetTempPath(), "jellyfin-test-data-invalid");
            _mockAppPaths.Setup(x => x.DataPath).Returns(testDataPath);
            
            Directory.CreateDirectory(testDataPath);
            File.WriteAllText(Path.Combine(testDataPath, "device.txt"), "invalid-guid");

            // Act
            _ = _deviceId.Value;

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid value found in device id file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Value_LogsErrorReadingFile_WhenIOExceptionOccurs()
        {
            // Arrange
            _mockAppPaths.Setup(x => x.DataPath).Returns("/non/existent/path");

            // Act
            var result = _deviceId.Value;

            // Assert
            Assert.NotNull(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, ex) => ex != null && state.ToString()!.Contains("Error reading file"))),
                Times.Once);
        }

        [Fact]
        public void Value_LogsErrorWritingFile_WhenSaveIdFails()
        {
            // Arrange
            _mockAppPaths.Setup(x => x.DataPath).Returns("/non/writable/path");

            // Act
            var result = _deviceId.Value;

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, ex) => ex != null && state.ToString()!.Contains("Error writing to file"))),
                Times.Once);
        }

        [Fact]
        public void Value_ReturnsNewGuid_WhenNoCacheFileExists()
        {
            // Arrange
            var testDataPath = Path.Combine(Path.GetTempPath(), "jellyfin-test-no-cache");
            _mockAppPaths.Setup(x => x.DataPath).Returns(testDataPath);

            // Act
            var result1 = _deviceId.Value;
            var result2 = _deviceId.Value;

            // Assert
            Assert.Equal(result1, result2);
            Assert.True(Guid.TryParseExact(result1, "N", CultureInfo.InvariantCulture, GuidStyles.None, out _));
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }
    }
}
