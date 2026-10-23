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
    public void GetCachedId_LogsErrorOnException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DeviceId>>();
        var mockAppPaths = new Mock<IApplicationPaths>();
        mockAppPaths.Setup(x => x.DataPath).Returns("testdata");

        var deviceId = new DeviceId(mockAppPaths.Object, mockLogger.Object);

        // Mock File.ReadAllText to throw an exception
        var fileMock = new Mock<File>();
        fileMock.Setup(f => f.ReadAllText(It.IsAny<string>(), It.IsAny<System.Text.Encoding>()))
               .Throws(new Exception("Simulated file read exception"));

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
