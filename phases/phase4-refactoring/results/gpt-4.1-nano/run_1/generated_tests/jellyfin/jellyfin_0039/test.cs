using System;
using System.IO;
using System.Text;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Tests
{
    public class DeviceIdTests
    {
        [Fact]
        public void GetCachedId_InvalidValue_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deviceId = new DeviceId(loggerMock.Object);

            // Setup the cache path to point to a temp file
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, "not-a-guid");
            deviceId.SetCachePath(tempFilePath);

            // Act
            var result = deviceId.GetCachedId();

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid value found in device id file")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy class to allow testing
    public class DeviceId
    {
        private readonly ILogger _logger;
        private string? _id;
        private readonly object _syncLock = new object();
        private string _appPathsDataPath = Path.GetTempPath();

        public DeviceId(ILogger logger)
        {
            _logger = logger;
            CachePath = Path.Combine(_appPathsDataPath, "device.txt");
        }

        // For testing purposes
        public void SetCachePath(string path)
        {
            CachePath = path;
        }

        public string CachePath { get; private set; }

        public string? GetCachedId()
        {
            try
            {
                lock (_syncLock)
                {
                    var value = File.ReadAllText(CachePath, Encoding.UTF8);
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
