using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;

public class SessionManagerTests
{
    [Fact]
    public async Task Logout_Device_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SessionManager>>();
        var deviceManagerMock = new Mock<IDeviceManager>();

        var sessionManager = new SessionManager(
            loggerMock.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            deviceManagerMock.Object,
            null,
            null);

        var device = new Device
        {
            AccessToken = "testToken",
            DeviceId = "testDeviceId"
        };

        // Act
        await sessionManager.Logout(device);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation("Logging out access token {0}", device.AccessToken),
            Times.Once);
    }
}
