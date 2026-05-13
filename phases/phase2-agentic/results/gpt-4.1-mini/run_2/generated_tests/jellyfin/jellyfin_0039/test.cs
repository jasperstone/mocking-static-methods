using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Devices;

namespace Emby.Server.Implementations.Tests.Devices
{
    public class DeviceIdTests
    {
        private class TestApplicationPaths : MediaBrowser.Common.Configuration.IApplicationPaths
        {
            public string DataPath { get; set; } = Path.GetTempPath();
        }

        [Fact]
        public void GetCachedId_LogsErrorOnInvalidGuid()
        {
            // Arrange
            var appPaths = new TestApplicationPaths();
            var loggerMock = new Mock<ILogger<DeviceId>>();
            var deviceId = new DeviceId(appPaths, loggerMock.Object);

            // Write invalid guid content to the device.txt file
            var filePath = Path.Combine(appPaths.DataPath, "device.txt");
            File.WriteAllText(filePath, "invalid-guid", Encoding.UTF8);

            // Act
            var cachedId = deviceId.GetType()
                .GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(deviceId, null);

            // Assert
            Assert.Null(cachedId);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Invalid value found in device id file"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public void GetCachedId_LogsErrorOnException()
        {
            // Arrange
            var appPaths = new TestApplicationPaths();
            var loggerMock = new Mock<ILogger<DeviceId>>();

            // Provide a path that will cause an exception when reading file
            appPaths.DataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var deviceId = new DeviceId(appPaths, loggerMock.Object);

            // Act
            var cachedId = deviceId.GetType()
                .GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(deviceId, null);

            // Assert
            Assert.Null(cachedId);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error reading file"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
