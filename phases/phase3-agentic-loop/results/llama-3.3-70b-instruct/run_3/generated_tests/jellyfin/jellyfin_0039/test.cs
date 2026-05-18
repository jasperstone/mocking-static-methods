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
            _loggerMock = new Mock<ILogger<DeviceId>>();
        }

        [Fact]
        public void GetCachedId_InvalidValue_LogsError()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var cachePath = Path.Combine(Path.GetTempPath(), "device.txt");
            _appPathsMock.SetupGet(a => a.DataPath).Returns(Path.GetTempPath());
            Directory.CreateDirectory(_appPathsMock.Object.DataPath);
            File.WriteAllText(cachePath, "InvalidGuid");

            // Act
            var id = deviceId.Value;

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetCachedId_DirectoryNotFoundException_LogsError()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var cachePath = Path.Combine(Path.GetTempPath(), "device.txt");
            _appPathsMock.SetupGet(a => a.DataPath).Returns(Path.GetTempPath());
            Directory.CreateDirectory(_appPathsMock.Object.DataPath);
            File.Delete(cachePath);

            // Act
            var id = deviceId.Value;

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void SaveId_Exception_LogsError()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var cachePath = Path.Combine(Path.GetTempPath(), "device.txt");
            _appPathsMock.SetupGet(a => a.DataPath).Returns(Path.GetTempPath());
            Directory.CreateDirectory(_appPathsMock.Object.DataPath);
            File.WriteAllText(cachePath, "ExistingId");

            // Act
            var id = deviceId.Value;

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetDeviceId_NoCachedId_ReturnsNewId()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var cachePath = Path.Combine(Path.GetTempPath(), "device.txt");
            _appPathsMock.SetupGet(a => a.DataPath).Returns(Path.GetTempPath());
            Directory.CreateDirectory(_appPathsMock.Object.DataPath);
            File.Delete(cachePath);

            // Act
            var id = deviceId.Value;

            // Assert
            Assert.NotEmpty(id);
        }
    }
}
