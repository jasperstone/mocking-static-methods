using System;
using System.IO;
using System.Text;
using System.Reflection;
using Emby.Server.Implementations.Devices;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Devices.Tests
{
    public class DeviceIdTests
    {
        private readonly Mock<IApplicationPaths> _appPathsMock;
        private readonly Mock<ILogger<DeviceId>> _loggerMock;
        private readonly string _tempDir;

        public DeviceIdTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);

            _appPathsMock = new Mock<IApplicationPaths>();
            _appPathsMock.Setup(ap => ap.DataPath).Returns(_tempDir);

            _loggerMock = new Mock<ILogger<DeviceId>>();
        }

        [Fact]
        public void GetCachedId_InvalidGuid_LogsError()
        {
            // Arrange
            var filePath = Path.Combine(_tempDir, "device.txt");
            File.WriteAllText(filePath, "invalid-guid", Encoding.UTF8);

            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Act
            var value = deviceId.Value;

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid value found in device id file")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetCachedId_ThrowsException_LogsError()
        {
            // Arrange
            var nonExistentDir = Path.Combine(_tempDir, "nonexistent");
            _appPathsMock.Setup(ap => ap.DataPath).Returns(nonExistentDir);

            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Act
            var value = deviceId.Value;

            // Assert
            // DirectoryNotFoundException is caught silently, so no error log expected here
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void SaveId_ThrowsException_LogsError()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Setup DataPath to a directory that causes Directory.CreateDirectory to throw
            // We create a file instead of directory to cause IOException on CreateDirectory
            var filePath = Path.Combine(_tempDir, "device.txt");
            File.WriteAllText(filePath, "dummy content");
            _appPathsMock.Setup(ap => ap.DataPath).Returns(filePath);

            // Act
            var saveIdMethod = typeof(DeviceId).GetMethod("SaveId", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(saveIdMethod);

            saveIdMethod.Invoke(deviceId, new object[] { "someid" });

            // Assert
            _loggerMock.Verify(
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
