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
        private class DummyDeviceId : DeviceId
        {
            public DummyDeviceId(ILogger logger, string dataPath) : base(logger, dataPath) { }

            public void TriggerGetCachedId() => GetCachedId();

            public void TriggerSaveId(string id) => SaveId(id);
        }

        [Fact]
        public void GetCachedId_InvalidValue_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var tempPath = Path.GetTempPath();
            var deviceId = new DummyDeviceId(mockLogger.Object, tempPath);
            var cacheFilePath = Path.Combine(tempPath, "device.txt");
            File.WriteAllText(cacheFilePath, "not-a-guid");

            // Act
            var result = deviceId.TriggerGetCachedId();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid value found in device id file")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
