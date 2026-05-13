using Xunit;
using Moq;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Devices;
using System.IO;
using System;

namespace Emby.Server.Implementations.Devices.Tests
{
    public class DeviceIdTests
    {
        private readonly Mock<IApplicationPaths> _appPathsMock;
        private readonly Mock<ILogger<DeviceId>> _loggerMock;
        private readonly DeviceId _deviceId;

        public DeviceIdTests()
        {
            _appPathsMock = new Mock<IApplicationPaths>();
            _loggerMock = new Mock<ILogger<DeviceId>>();
            _deviceId = new DeviceId(_appPathsMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetCachedId_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new Exception("Test exception");
            _appPathsMock.Setup(x => x.DataPath).Returns("test/path");
            _loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ));

            // Act
            var result = _deviceId.GetType().GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(_deviceId, null);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
