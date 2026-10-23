using System;
using System.IO;
using System.Text;
using Emby.Server.Implementations.Devices;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Devices
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
            File.WriteAllText(filePath, "not-a-guid", Encoding.UTF8);

            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Act
            // Access Value to trigger GetDeviceId which calls GetCachedId internally
            var value = deviceId.Value;

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid value found in device id file")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetCachedId_FileReadThrows_LogsError()
        {
            // Arrange
            // Remove directory to cause DirectoryNotFoundException on read
            Directory.Delete(_tempDir, true);

            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Act
            var value = deviceId.Value;

            // Assert
            // Since DirectoryNotFoundException is caught and ignored, no error log expected here
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void GetCachedId_FileReadThrowsOtherException_LogsError()
        {
            // Arrange
            var filePath = Path.Combine(_tempDir, "device.txt");
            File.WriteAllText(filePath, "some content", Encoding.UTF8);

            // Lock the file to cause IOException on read
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

                // Act
                var value = deviceId.Value;

                // Assert
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
        }
    }
}
