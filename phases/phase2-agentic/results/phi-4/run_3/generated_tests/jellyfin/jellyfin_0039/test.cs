using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Xunit;
using Emby.Server.Implementations.Devices;

public class DeviceIdTests
{
    [Fact]
    public void GetCachedId_LogsError_WhenExceptionThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DeviceId>>();
        var appPaths = new Mock<IApplicationPaths>();
        appPaths.Setup(p => p.DataPath).Returns("testPath");

        var deviceId = new DeviceId(appPaths.Object, mockLogger.Object);

        // Simulate an exception during file reading
        var exception = new IOException("Test exception");
        File.ReadAllText = () => throw exception;

        // Act
        var result = deviceId.GetCachedId();

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(It.IsAny<Exception>(), "Error reading file"),
            Times.Once
        );

        Assert.Null(result);
    }
}
