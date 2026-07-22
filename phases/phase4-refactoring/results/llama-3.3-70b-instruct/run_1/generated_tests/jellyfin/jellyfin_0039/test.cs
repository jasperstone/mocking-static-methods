using Emby.Server.Implementations.Devices;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Emby.Server.Implementations.Devices.Tests
{
    public class DeviceIdTests
    {
        private readonly Mock<ILogger<DeviceId>> _loggerMock;
        private readonly Mock<IApplicationPaths> _appPathsMock;

        public DeviceIdTests()
        {
            _loggerMock = new Mock<ILogger<DeviceId>>();
            _appPathsMock = new Mock<IApplicationPaths>();
        }

        [Fact]
        public void GetCachedId_LogsError_WhenFileReadFails()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            _appPathsMock.SetupGet(x => x.DataPath).Returns("path");
            var cachePath = Path.Combine("path", "device.txt");

            // Act and Assert
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                var id = deviceId.Value;
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
            }
        }

        [Fact]
        public void GetCachedId_LogsError_WhenGuidParseFails()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            _appPathsMock.SetupGet(x => x.DataPath).Returns("path");
            var cachePath = Path.Combine("path", "device.txt");
            File.WriteAllText(cachePath, "InvalidGuid");

            // Act and Assert
            var id = deviceId.Value;
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);

            // Cleanup
            File.Delete(cachePath);
        }

        [Fact]
        public void SaveId_LogsError_WhenFileWriteFails()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            _appPathsMock.SetupGet(x => x.DataPath).Returns("path");
            var cachePath = Path.Combine("path", "device.txt");

            // Act and Assert
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                var id = deviceId.Value;
                _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
            }
        }
    }
}
