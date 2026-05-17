using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Emby.Server.Implementations.Devices;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Devices
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
        public void ValuePropertyTriggersGetDeviceIdCaching()
        {
            // Arrange
            _mockAppPaths.Setup(p => p.DataPath).Returns(Path.GetTempPath());

            // Act
            var firstValue = _deviceId.Value;
            var secondValue = _deviceId.Value;

            // Assert - same value due to caching
            Assert.Equal(firstValue, secondValue);
        }

        [Fact]
        public void GetCachedId_InvalidGuidInFile_LogsInvalidValueFoundError()
        {
            // Arrange
            string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string testPath = Path.Combine(testDir, "device.txt");
            _mockAppPaths.Setup(p => p.DataPath).Returns(testDir);
            
            Directory.CreateDirectory(testDir);
            File.WriteAllText(testPath, "invalid-guid", Encoding.UTF8);

            try
            {
                // Act - accessing Value will call GetCachedId
                _ = _deviceId.Value;

                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid value found in device id file")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                if (Directory.Exists(testDir))
                    Directory.Delete(testDir, true);
            }
        }

        [Fact]
        public void GetCachedId_ThrowsException_LogsErrorReadingFile()
        {
            // Arrange - delete file after lock acquisition by using reflection to call GetCachedId directly
            string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string testPath = Path.Combine(testDir, "device.txt");
            _mockAppPaths.Setup(p => p.DataPath).Returns(testDir);
            
            Directory.CreateDirectory(testDir);
            File.WriteAllText(testPath, Guid.NewGuid().ToString("N"), Encoding.UTF8);

            var getCachedIdMethod = typeof(DeviceId).GetMethod("GetCachedId", BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            try
            {
                // Delete file while lock is held inside GetCachedId
                _ = Task.Run(() =>
                {
                    while (File.Exists(testPath))
                    {
                        try
                        {
                            File.Delete(testPath);
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(10);
                        }
                    }
                }).Result;

                // Act
                getCachedIdMethod.Invoke(_deviceId, null);

                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error reading file")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                if (Directory.Exists(testDir))
                    Directory.Delete(testDir, true);
            }
        }

        [Fact]
        public void SaveId_ThrowsException_LogsErrorWritingToFile()
        {
            // Arrange - use reflection to call SaveId directly with invalid path
            _mockAppPaths.Setup(p => p.DataPath).Returns(@"\\.\InvalidPath");
            string validId = Guid.NewGuid().ToString("N");
            
            var saveIdMethod = typeof(DeviceId).GetMethod("SaveId", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            saveIdMethod.Invoke(_deviceId, new object[] { validId });

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error writing to file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
