using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_Device_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();

            // Create mocks for all required constructor dependencies using object
            var mocks = new[]
            {
                new Mock<IUserDataManager>().Object,
                new Mock<IServerConfigurationManager>().Object,
                new Mock<IEventManager>().Object,
                new Mock<ILibraryManager>().Object,
                new Mock<IUserManager>().Object,
                new Mock<IMusicManager>().Object,
                new Mock<IDtoService>().Object,
                new Mock<IImageProcessor>().Object,
                new Mock<IServerApplicationHost>().Object,
                deviceManagerMock.Object,
                new Mock<IMediaSourceManager>().Object,
                new Mock<IHostApplicationLifetime>().Object
            };

            var sessionManager = new SessionManager(
                loggerMock.Object,
                (IEventManager)mocks[1],
                (IUserDataManager)mocks[0],
                (IServerConfigurationManager)mocks[2],
                (ILibraryManager)mocks[3],
                (IUserManager)mocks[4],
                (IMusicManager)mocks[5],
                (IDtoService)mocks[6],
                (IImageProcessor)mocks[7],
                (IServerApplicationHost)mocks[8],
                (IDeviceManager)mocks[9],
                (IMediaSourceManager)mocks[10],
                (IHostApplicationLifetime)mocks[11]);

            var device = new MediaBrowser.Model.Devices.Device
            {
                AccessToken = "test-access-token",
                DeviceId = "test-device-id"
            };

            deviceManagerMock
                .Setup(m => m.DeleteDevice(It.IsAny<MediaBrowser.Model.Devices.Device>()))
                .Returns(Task.CompletedTask);

            // Act
            await sessionManager.Logout(device);

            // Assert - Verify LogInformation call on line 1723
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Logging out access token test-access-token")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
