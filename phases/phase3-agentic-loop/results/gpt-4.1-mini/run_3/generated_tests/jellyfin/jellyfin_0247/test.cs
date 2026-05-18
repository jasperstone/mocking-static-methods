using System;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Devices;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Hosting;

namespace Emby.Server.Implementations.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task Logout_Device_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
            var cts = new System.Threading.CancellationTokenSource();
            hostApplicationLifetimeMock.Setup(h => h.ApplicationStopping).Returns(cts.Token);

            // Setup deviceManager.DeleteDevice to return completed task
            deviceManagerMock.Setup(d => d.DeleteDevice(It.IsAny<Device>())).Returns(Task.CompletedTask);

            // Create SessionManager instance with minimal mocks for other parameters
            var sessionManager = new SessionManager(
                loggerMock.Object,
                eventManager: null,
                userDataManager: null,
                serverConfigurationManager: null,
                libraryManager: null,
                userManager: null,
                musicManager: null,
                dtoService: null,
                imageProcessor: null,
                appHost: null,
                deviceManagerMock.Object,
                mediaSourceManager: null,
                hostApplicationLifetimeMock.Object);

            // Create a device with an access token
            var device = new Device
            {
                AccessToken = "test-access-token",
                DeviceId = "device-id-123"
            };

            // Act
            await sessionManager.Logout(device);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Logging out access token test-access-token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
