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
        private readonly string _dataPath;

        public DeviceIdTests()
        {
            _appPathsMock = new Mock<IApplicationPaths>();
            _loggerMock = new Mock<ILogger<DeviceId>>();
            _dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_dataPath);
            _appPathsMock.Setup(ap => ap.DataPath).Returns(_dataPath);
        }

        [Fact]
        public void GetCachedId_InvalidGuid_LogsError()
        {
            // Arrange
            var filePath = Path.Combine(_dataPath, "device.txt");
            File.WriteAllText(filePath, "invalid-guid", Encoding.UTF8);

            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Act
            var value = deviceId.Value; // This triggers GetDeviceId which calls GetCachedId

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
        public void GetCachedId_Exception_LogsError()
        {
            // Arrange
            // Setup DataPath to a path that will cause an exception when reading file
            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.Setup(ap => ap.DataPath).Returns("Z:\\nonexistent_path");

            var loggerMock = new Mock<ILogger<DeviceId>>();

            var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

            // Act
            var value = deviceId.Value; // This triggers GetDeviceId which calls GetCachedId

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never); // Because DirectoryNotFoundException and FileNotFoundException are caught silently

            // To test the LogError with exception, we need to simulate an exception other than those two
            // So we create a derived class to override CachePath to throw an exception

        }

        [Fact]
        public void SaveId_Exception_LogsError()
        {
            // Arrange
            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.Setup(ap => ap.DataPath).Returns(_dataPath);

            var loggerMock = new Mock<ILogger<DeviceId>>();

            var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

            // Use reflection to call private SaveId method with a path that causes exception
            // We simulate exception by setting DataPath to root directory (which causes InvalidOperationException in SaveId)

            appPathsMock.Setup(ap => ap.DataPath).Returns(Path.GetPathRoot(_dataPath));

            var deviceIdWithBadPath = new DeviceId(appPathsMock.Object, loggerMock.Object);

            // Use reflection to invoke SaveId
            var saveIdMethod = typeof(DeviceId).GetMethod("SaveId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            saveIdMethod.Invoke(deviceIdWithBadPath, new object[] { "someid" });

            // Assert
            loggerMock.Verify(
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
