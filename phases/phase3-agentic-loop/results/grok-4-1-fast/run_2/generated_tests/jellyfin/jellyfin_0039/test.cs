using System;
using System.Globalization;
using System.IO;
using System.Text;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Devices.Tests
{
    public class DeviceIdTests
    {
        private readonly Mock<IApplicationPaths> _mockAppPaths;
        private readonly Mock<ILogger<DeviceId>> _mockLogger;
        private readonly DeviceId _deviceId;

        public DeviceIdTests()
        {
            _mockAppPaths = new Mock<IApplicationPaths>();
            _mockLogger = new Mock<ILogger<DeviceId>>();
            _deviceId = new DeviceId(_mockAppPaths.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetCachedId_FileReadThrowsException_LogsErrorReadingFile()
        {
            // Arrange
            _mockAppPaths.Setup(p => p.DataPath).Returns("/nonexistent/path");

            // Act
            _deviceId.Value;

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error reading file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void SaveId_ThrowsException_LogsErrorWritingToFile()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _mockAppPaths.Setup(p => p.DataPath).Returns(Path.Combine(tempDir, "protected"));

            // Make directory read-only to force SaveId exception
            Directory.CreateDirectory(tempDir);
            var protectedDir = Path.Combine(tempDir, "protected");
            Directory.CreateDirectory(protectedDir);
            new DirectoryInfo(protectedDir).Attributes |= FileAttributes.ReadOnly;

            try
            {
                // Act - forces new ID generation -> SaveId
                _deviceId.Value;
            }
            finally
            {
                try
                {
                    new DirectoryInfo(protectedDir).Attributes &= ~FileAttributes.ReadOnly;
                }
                catch { }
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error writing to file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void GetCachedId_InvalidGuidFormat_LogsInvalidValueFound()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _mockAppPaths.Setup(p => p.DataPath).Returns(tempDir);
            var cachePath = Path.Combine(tempDir, "device.txt");
            
            Directory.CreateDirectory(tempDir);
            File.WriteAllText(cachePath, "invalid-guid", Encoding.UTF8);

            try
            {
                // Act
                _deviceId.Value;

                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString() == "Invalid value found in device id file"),
                        It.IsNull<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Exactly(1));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}
