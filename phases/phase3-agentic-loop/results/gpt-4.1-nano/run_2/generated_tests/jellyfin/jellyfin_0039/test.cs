using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Devices;

namespace Emby.Tests
{
    public class DeviceIdTests
    {
        private readonly Mock<IApplicationPaths> _appPathsMock;
        private readonly Mock<ILogger<DeviceId>> _loggerMock;
        private readonly string _tempDir;

        public DeviceIdTests()
        {
            _appPathsMock = new Mock<IApplicationPaths>();
            _loggerMock = new Mock<ILogger<DeviceId>>();
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _appPathsMock.Setup(p => p.DataPath).Returns(_tempDir);
        }

        [Fact]
        public void GetCachedId_InvalidGuidLogsError()
        {
            // Arrange
            var invalidContent = "not-a-guid";
            var filePath = Path.Combine(_tempDir, "device.txt");
            File.WriteAllText(filePath, invalidContent, Encoding.UTF8);
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Act
            var result = deviceId.GetType().GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(deviceId, null);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid value found in device id file")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void GetCachedId_FileNotFound_ReturnsNull()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var nonExistentPath = Path.Combine(_tempDir, "nonexistent.txt");
            // Override CachePath property
            var cachePathProp = typeof(DeviceId).GetProperty("CachePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var originalCachePath = cachePathProp.GetValue(deviceId);
            cachePathProp.SetValue(deviceId, nonExistentPath);

            // Act
            var result = deviceId.GetType().GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(deviceId, null);

            // Reset CachePath
            cachePathProp.SetValue(deviceId, originalCachePath);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCachedId_ThrowsException_LogsError()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            var filePath = Path.Combine(_tempDir, "device.txt");
            File.WriteAllText(filePath, "some content", Encoding.UTF8);
            // Override CachePath to simulate exception
            var cachePathProp = typeof(DeviceId).GetProperty("CachePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var originalCachePath = cachePathProp.GetValue(deviceId);
            cachePathProp.SetValue(deviceId, null); // cause exception in File.ReadAllText

            // Act
            var result = deviceId.GetType().GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(deviceId, null);

            // Reset CachePath
            cachePathProp.SetValue(deviceId, originalCachePath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error reading file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void LogErrorOnExceptionInGetDeviceId()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
            // Since GetDeviceId is private, test indirectly: simulate GetCachedId returning null, then ensure SaveId logs error if it throws
            // To do this, override SaveId to throw
            var deviceIdType = typeof(DeviceId);
            var saveIdMethod = deviceIdType.GetMethod("SaveId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var originalSaveId = saveIdMethod;
            // Create a subclass to override SaveId
            var deviceIdSub = new TestDeviceId(_appPathsMock.Object, _loggerMock.Object);
            deviceIdSub.ForceSaveIdException = true;

            // Act
            var id = deviceIdSub.GetDeviceId();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error writing to file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestDeviceId : DeviceId
        {
            public bool ForceSaveIdException { get; set; } = false;

            public TestDeviceId(IApplicationPaths appPaths, ILogger<DeviceId> logger) : base(appPaths, logger)
            {
            }

            protected override void SaveId(string id)
            {
                if (ForceSaveIdException)
                {
                    throw new Exception("Simulated save exception");
                }
                base.SaveId(id);
            }
        }
    }
}
