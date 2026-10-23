using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Model.Session;
using System.Collections.Generic;
using System.Linq;

namespace Emby.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_Device_ShouldLogInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var eventManagerMock = new Mock<IEventManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var userManagerMock = new Mock<IUserManager>();
            var musicManagerMock = new Mock<IMusicManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var hostLifetimeMock = new Mock<IHostApplicationLifetime>();

            var sessionManager = new SessionManager(
                loggerMock.Object,
                eventManagerMock.Object,
                userDataManagerMock.Object,
                serverConfigMock.Object,
                libraryManagerMock.Object,
                userManagerMock.Object,
                musicManagerMock.Object,
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                appHostMock.Object,
                deviceManagerMock.Object,
                mediaSourceManagerMock.Object,
                hostLifetimeMock.Object
            );

            var testDevice = new Device
            {
                Id = "device1",
                AccessToken = "token123",
                DeviceId = "device1"
            };

            // Setup device manager to return a device
            deviceManagerMock.Setup(dm => dm.GetDevices(It.IsAny<DeviceQuery>()))
                .Returns(new DeviceQueryResult
                {
                    Items = new List<Device> { testDevice }
                });

            // Act
            await sessionManager.Logout(testDevice.AccessToken);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Logging out access token {0}", testDevice.AccessToken),
                Times.Once);
        }
    }
}
