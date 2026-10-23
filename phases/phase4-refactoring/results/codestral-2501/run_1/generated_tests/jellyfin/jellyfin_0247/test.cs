using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Devices;
using Jellyfin.Database.Implementations.Entities.Security;
using Jellyfin.Data.Queries;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using System.Collections.Generic;
using MediaBrowser.Model.Querying;

public class SessionManagerTests
{
    [Fact]
    public async Task Logout_ShouldLogInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SessionManager>>();
        var mockDeviceManager = new Mock<IDeviceManager>();
        var device = new Device(Guid.NewGuid(), "app", "appVersion", "deviceName", "deviceId") { AccessToken = "testToken" };
        var deviceQueryResult = new QueryResult<Device> { Items = new List<Device> { device } };

        mockDeviceManager.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>())).Returns(deviceQueryResult);
        mockDeviceManager.Setup(dm => dm.DeleteDevice(It.IsAny<Device>())).Returns(Task.CompletedTask);

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
            logger => logger.LogInformation("Logging out access token {0}", "testToken"),
            Times.Once);
    }
}
