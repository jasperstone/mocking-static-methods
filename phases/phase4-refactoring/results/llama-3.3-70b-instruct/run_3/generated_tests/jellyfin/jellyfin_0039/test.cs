using Emby.Server.Implementations.Devices;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Threading;
using Xunit;

namespace Emby.Server.Implementations.Devices.Tests
{
    public class DeviceIdTests
    {
        [Fact]
        public void GetCachedId_LogsError_WhenFileReadFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DeviceId>>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            appPathsMock.SetupGet(p => p.DataPath).Returns("/path/to/data");
            var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

            // Make the file read fail
            var cachePath = Path.Combine(appPathsMock.Object.DataPath, "device.txt");
            File.Create(cachePath).Dispose();

            // Act
            var id = deviceId.Value;

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error reading file"), Times.Once);
        }

        [Fact]
        public void GetCachedId_LogsError_WhenFileContainsInvalidGuid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DeviceId>>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            appPathsMock.SetupGet(p => p.DataPath).Returns("/path/to/data");
            var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

            // Make the file contain an invalid GUID
            var cachePath = Path.Combine(appPathsMock.Object.DataPath, "device.txt");
            File.WriteAllText(cachePath, "InvalidGuid");

            // Act
            var id = deviceId.Value;

            // Assert
            loggerMock.Verify(l => l.LogError("Invalid value found in device id file"), Times.Once);
        }

        [Fact]
        public void SaveId_LogsError_WhenFileWriteFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DeviceId>>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            appPathsMock.SetupGet(p => p.DataPath).Returns("/path/to/data");
            var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

            // Make the file write fail
            var cachePath = Path.Combine(appPathsMock.Object.DataPath, "device.txt");
            Directory.CreateDirectory(appPathsMock.Object.DataPath);
            File.Create(cachePath).Dispose();

            // Act
            var id = deviceId.Value;

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error writing to file"), Times.Once);
        }
    }
}
