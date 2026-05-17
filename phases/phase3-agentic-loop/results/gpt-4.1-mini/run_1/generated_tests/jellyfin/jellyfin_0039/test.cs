using System;
using System.IO;
using System.Text;
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
        public void GetCachedId_LogsError_WhenFileContainsInvalidGuid()
        {
            // Arrange
            var invalidContent = "not-a-guid";
            var filePath = Path.Combine(_tempDir, "device.txt");
            File.WriteAllText(filePath, invalidContent, Encoding.UTF8);

            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Act
            var value = deviceId.Value; // This triggers GetDeviceId which calls GetCachedId internally

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
        public void GetCachedId_DoesNotLogError_WhenDirectoryNotFoundException()
        {
            // Arrange
            // Setup appPaths to a directory that does not exist to cause DirectoryNotFoundException
            var nonExistentDir = Path.Combine(_tempDir, "nonexistent");
            _appPathsMock.Setup(ap => ap.DataPath).Returns(nonExistentDir);

            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Act
            var value = deviceId.Value; // This triggers GetDeviceId which calls GetCachedId internally

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
        public void GetCachedId_LogsError_WhenExceptionThrownDuringRead()
        {
            // Arrange
            // Setup appPaths to a directory that causes an unexpected exception when reading file
            // We simulate this by mocking File.ReadAllText to throw an exception using a shim
            // Since we cannot mock static File.ReadAllText easily, we simulate by setting DataPath to a file path (not directory)
            var filePath = Path.Combine(_tempDir, "device.txt");
            File.WriteAllText(filePath, "some content", Encoding.UTF8);
            _appPathsMock.Setup(ap => ap.DataPath).Returns(filePath); // This will cause Path.Combine to be invalid and throw

            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Act
            var value = deviceId.Value; // This triggers GetDeviceId which calls GetCachedId internally

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

        [Fact]
        public void SaveId_LogsError_WhenExceptionThrownDuringWrite()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Setup appPaths to a root directory to cause InvalidOperationException in SaveId
            _appPathsMock.Setup(ap => ap.DataPath).Returns(Path.GetPathRoot(_tempDir));

            // Act
            var id = Guid.NewGuid().ToString("N");
            // Call SaveId via reflection since it's private
            var saveIdMethod = typeof(DeviceId).GetMethod("SaveId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            saveIdMethod.Invoke(deviceId, new object[] { id });

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
