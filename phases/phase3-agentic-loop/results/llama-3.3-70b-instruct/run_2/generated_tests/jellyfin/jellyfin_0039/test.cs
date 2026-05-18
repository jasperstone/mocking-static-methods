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
            var cachePath = Path.Combine(_appPathsMock.Object.DataPath, "device.txt");
            File.WriteAllText(cachePath, "InvalidGuid");

            // Act
            var value = deviceId.Value;

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetCachedId_DirectoryNotFoundException_DoesNotLogError()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            _appPathsMock.SetupGet(a => a.DataPath).Returns(string.Empty);

            // Act
            var value = deviceId.Value;

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetCachedId_FileNotFoundException_DoesNotLogError()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var cachePath = Path.Combine(_appPathsMock.Object.DataPath, "device.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath));

            // Act
            var value = deviceId.Value;

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetCachedId_Exception_LogsError()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var cachePath = Path.Combine(_appPathsMock.Object.DataPath, "device.txt");
            File.WriteAllText(cachePath, "ValidGuid");

            // Act and Assert
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                Console.SetError(sw);
                try
                {
                    File.Delete(cachePath);
                    var value = deviceId.Value;
                }
                catch (Exception ex)
                {
                    _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
                }
            }
        }

        [Fact]
        public void SaveId_Exception_LogsError()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var cachePath = Path.Combine(_appPathsMock.Object.DataPath, "device.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
            File.SetAttributes(cachePath, FileAttributes.ReadOnly);

            // Act
            var value = deviceId.Value;

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}
