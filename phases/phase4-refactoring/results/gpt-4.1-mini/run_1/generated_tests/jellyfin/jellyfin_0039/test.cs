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
    public class DeviceIdTests : IDisposable
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
        public void GetCachedId_LogsErrorOnInvalidGuidValue()
        {
            // Arrange
            var invalidContent = "not-a-guid";
            var filePath = Path.Combine(_tempDir, "device.txt");
            File.WriteAllText(filePath, invalidContent, Encoding.UTF8);

            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Act
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
        public void GetCachedId_DoesNotLogErrorOnDirectoryNotFound()
        {
            // Arrange
            var nonExistentDir = Path.Combine(_tempDir, "nonexistent");
            _appPathsMock.Setup(ap => ap.DataPath).Returns(nonExistentDir);

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
                Times.Never);
        }

        [Fact]
        public void SaveId_DoesNotLogErrorOnValidPath()
        {
            // Arrange
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
                Times.Never);
        }

        [Fact]
        public void SaveId_LogsErrorOnInvalidPath()
        {
            // Arrange
            var deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);

            // Setup DataPath to a root directory which will cause Path.GetDirectoryName to return null,
            // triggering InvalidOperationException in SaveId, but catch block should handle it and log error.
            _appPathsMock.Setup(ap => ap.DataPath).Returns(Path.GetPathRoot(_tempDir));

            // Act
            // Clear cached id to force SaveId call
            var field = typeof(DeviceId).GetField("_id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(deviceId, null);

            var value = deviceId.Value;

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (Directory.Exists(_tempDir))
                        Directory.Delete(_tempDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}
