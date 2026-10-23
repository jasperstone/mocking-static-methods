using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Devices;
using MediaBrowser.Common.Configuration;
using System.IO;
using System;

public class DeviceIdTests
{
    [Fact]
    public void GetCachedId_LogsError_WhenExceptionOccurs()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DeviceId>>();
        var mockAppPaths = new Mock<IApplicationPaths>();
        mockAppPaths.Setup(x => x.DataPath).Returns("testdata");

        var deviceId = new DeviceId(mockAppPaths.Object, mockLogger.Object);

        // Simulate a file read exception
        var filePath = Path.Combine("testdata", "device.txt");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        Directory.CreateDirectory("testdata");

        // Act
        var result = deviceId.GetType().GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(deviceId, null);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
