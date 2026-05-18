using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Model.Devices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Emby.Server.Tests.Implementations.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_ShouldLogInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();

            var device = new Device
            {
                AccessToken = "testToken",
                DeviceId = "testDeviceId"
            };

            deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new QueryResult<Device> { Items = new List<Device> { device } });

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

            // Act
            await sessionManager.Logout("testToken");

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Logging out access token {0}", "testToken"),
                Times.Once);
        }
    }
}
