using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;
using Emby.Server.Implementations.Devices;
using System.IO;
using System.Text;

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

        // Act
        var result = CallGetCachedId();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error reading file")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.Null(result);

        string CallGetCachedId()
        {
            var method = typeof(DeviceId).GetMethod("GetCachedId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (string)method.Invoke(_deviceId, null);
        }
    }
}
