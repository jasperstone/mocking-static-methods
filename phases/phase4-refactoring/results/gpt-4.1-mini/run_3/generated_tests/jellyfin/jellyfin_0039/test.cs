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
    public sealed class DeviceIdTests : IDisposable
    {
        private readonly string _tempDir;

        public DeviceIdTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDir))
                {
                    Directory.Delete(_tempDir, true);
                }
            }
            catch
            {
                // ignore cleanup errors
            }
        }

        [Fact]
        public void GetCachedId_LogsError_WhenFileContainsInvalidGuid()
        {
            // Arrange
            var invalidContent = "not-a-guid";
            var filePath = Path.Combine(_tempDir, "device.txt");
            File.WriteAllText(filePath, invalidContent, Encoding.UTF8);

            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.Setup(ap => ap.DataPath).Returns(_tempDir);

            var loggerMock = new Mock<ILogger<DeviceId>>();

            var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

            // Act
            // Access Value to trigger GetDeviceId, which calls GetCachedId internally
            var value = deviceId.Value;

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid value found in device id file")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
