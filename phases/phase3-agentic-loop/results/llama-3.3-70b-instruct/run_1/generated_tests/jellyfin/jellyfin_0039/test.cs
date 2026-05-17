using Emby.Server.Implementations.Devices;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Emby.Server.Tests
{
    public class DeviceIdTests
    {
        private readonly Mock<IApplicationPaths> _appPathsMock;
        private readonly Mock<ILogger<DeviceId>> _loggerMock;

        public DeviceIdTests()
        {
            _appPathsMock = new Mock<IApplicationPaths>();
            _appPathsMock.SetupGet(a => a.DataPath).Returns("/test/data");
            _loggerMock = new Mock<ILogger<DeviceId>>();
        }

        [Fact]
        public void GetCachedId_LogsError_WhenFileReadFails()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var cachePath = Path.Combine(_appPathsMock.Object.DataPath, "device.txt");
            File.Create(cachePath).Dispose(); // Create the file to avoid FileNotFoundException

            // Act and Assert
            using (var stream = new FileStream(cachePath, FileMode.Open, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("InvalidGuid");
                }
            }

            var value = deviceId.Value;

            _loggerMock.Verify(logger => logger.LogError("Invalid value found in device id file"), Times.Once);
        }

        [Fact]
        public void GetCachedId_LogsError_WhenFileReadThrowsException()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var cachePath = Path.Combine(_appPathsMock.Object.DataPath, "device.txt");
            File.Create(cachePath).Dispose(); // Create the file to avoid FileNotFoundException

            // Act and Assert
            using (var stream = new FileStream(cachePath, FileMode.Open, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("InvalidGuid");
                }
            }

            // Make the file unreadable
            File.SetAttributes(cachePath, FileAttributes.ReadOnly);

            var value = deviceId.Value;

            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error reading file"), Times.Once);
        }

        [Fact]
        public void SaveId_LogsError_WhenFileWriteFails()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var cachePath = Path.Combine(_appPathsMock.Object.DataPath, "device.txt");

            // Make the directory unreadable
            Directory.CreateDirectory(_appPathsMock.Object.DataPath);
            new DirectoryInfo(_appPathsMock.Object.DataPath).Attributes = FileAttributes.ReadOnly;

            // Act and Assert
            var value = deviceId.Value;

            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error writing to file"), Times.Once);
        }
    }
}
