using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;
using Emby.Server.Implementations.Devices;
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

        // Act
        var result = deviceId.GetDeviceId();

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(s => s == "Error reading file")),
            Times.Once);
    }
}
