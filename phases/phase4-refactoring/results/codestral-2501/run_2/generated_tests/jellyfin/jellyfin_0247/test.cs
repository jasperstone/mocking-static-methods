using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Model.Devices;
using MediaBrowser.Model.Querying;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;

public class SessionManagerTests
{
    [Fact]
    public async Task Logout_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SessionManager>>();
        var mockDeviceManager = new Mock<IDeviceManager>();

        var device = new Device
        {
            AccessToken = "testToken",
            DeviceId = "testDeviceId"
        };

        mockDeviceManager.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
            .Returns(new QueryResult<Device> { Items = new[] { device } });

        var sessionManager = new SessionManager(
            mockLogger.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            mockDeviceManager.Object,
            null,
            null);

        // Act
        await sessionManager.Logout("testToken");

        // Assert
        mockLogger.Verify(
            x => x.LogInformation("Logging out access token {0}", "testToken"),
            Times.Once);
    }
}
