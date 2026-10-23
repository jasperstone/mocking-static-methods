using Emby.Server.Implementations.Devices;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Threading;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class DeviceIdTests
    {
        [Fact]
        public void GetCachedId_LogsError_WhenFileReadFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DeviceId>>();
            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.SetupGet(p => p.DataPath).Returns(Path.GetTempPath());
            var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

            // Act and Assert
            using (var stream = File.Create(Path.Combine(appPathsMock.Object.DataPath, "device.txt")))
            {
                stream.SetLength(10);
            }

            var cachedId = deviceId.Value;

            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error reading file"), Times.Once);
        }

        [Fact]
        public void GetCachedId_LogsError_WhenFileContainsInvalidGuid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DeviceId>>();
            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.SetupGet(p => p.DataPath).Returns(Path.GetTempPath());
            var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

            // Act and Assert
            File.WriteAllText(Path.Combine(appPathsMock.Object.DataPath, "device.txt"), "InvalidGuid");

            var cachedId = deviceId.Value;

            loggerMock.Verify(l => l.LogError("Invalid value found in device id file"), Times.Once);
        }

        [Fact]
        public void SaveId_LogsError_WhenFileWriteFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DeviceId>>();
            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.SetupGet(p => p.DataPath).Returns(Path.GetTempPath());
            var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

            // Act and Assert
            Directory.CreateDirectory(appPathsMock.Object.DataPath);
            File.Create(Path.Combine(appPathsMock.Object.DataPath, "device.txt")).Dispose();

            var id = deviceId.Value;

            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error writing to file"), Times.Once);
        }
    }
}
