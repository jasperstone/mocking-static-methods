using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests
{
    public class DeviceIdTests
    {
        private readonly Mock<ILogger<DeviceId>> _loggerMock;
        private readonly Mock<IAppPaths> _appPathsMock;

        public DeviceIdTests()
        {
            _loggerMock = new Mock<ILogger<DeviceId>>();
            _appPathsMock = new Mock<IAppPaths>();
        }

        [Fact]
        public void GetCachedId_InvalidValueInFile_LogsError()
        {
            // Arrange
            var invalidGuid = "not-a-guid";
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, invalidGuid, Encoding.UTF8);

            _appPathsMock.Setup(p => p.DataPath).Returns(Path.GetDirectoryName(tempFilePath));
            var deviceId = new DeviceId(_loggerMock.Object, _appPathsMock.Object);

            // Act
            var result = deviceId.GetCachedId();

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<string>()),
                Times.Once);
            // Cleanup
            File.Delete(tempFilePath);
        }
    }

    // Placeholder for IAppPaths interface
    public interface IAppPaths
    {
        string DataPath { get; }
    }

    // Placeholder for DeviceId class
    public class DeviceId
    {
        private readonly ILogger<DeviceId> _logger;
        private readonly IAppPaths _appPaths;
        private readonly object _syncLock = new object();
        private string _id;

        public DeviceId(ILogger<DeviceId> logger, IAppPaths appPaths)
        {
            _logger = logger;
            _appPaths = appPaths;
        }

        public string? GetCachedId()
        {
            var cachePath = Path.Combine(_appPaths.DataPath, "device.txt");
            try
            {
                lock (_syncLock)
                {
                    var value = File.ReadAllText(cachePath, Encoding.UTF8);
                    if (Guid.TryParse(value, out _))
                    {
                        return value;
                    }
                    _logger.LogError("Invalid value found in device id file");
                }
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file");
            }
            return null;
        }
    }
}
