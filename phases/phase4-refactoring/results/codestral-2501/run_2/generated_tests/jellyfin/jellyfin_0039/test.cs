using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;
using Emby.Server.Implementations.Devices;
using System.IO;
using System.Text;

public class DeviceIdTests
{
    [Fact]
    public void GetCachedId_LogsError_WhenExceptionOccurs()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DeviceId>>();
        var mockAppPaths = new Mock<IApplicationPaths>();
        mockAppPaths.Setup(p => p.DataPath).Returns("testdata");

        var deviceId = new DeviceId(mockAppPaths.Object, mockLogger.Object);

        // Act
        var result = deviceId.GetType().GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(deviceId, null);

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error reading file"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
