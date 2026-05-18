using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly Mock<IDisposable> _mockDisposable;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _mockDisposable = new Mock<IDisposable>();

            // Create mocks that implement IDisposable for the constructor
            var mockUserDataManager = new Mock<IUserDataManager>().Object;
            var mockConfig = new Mock<IServerConfigurationManager>().Object;
            var mockEventManager = new Mock<IEventManager>().Object;
            var mockLibraryManager = new Mock<ILibraryManager>().Object;
            var mockUserManager = new Mock<IUserManager>().Object;
            var mockMusicManager = new Mock<IMusicManager>().Object;
            var mockDtoService = new Mock<IDtoService>().Object;
            var mockImageProcessor = new Mock<IImageProcessor>().Object;
            var mockAppHost = new Mock<IServerApplicationHost>().Object;
            var mockDeviceManager = new Mock<IDeviceManager>().Object;
            var mockMediaSourceManager = new Mock<IMediaSourceManager>().Object;
            var mockHostLifetime = new Mock<IHostApplicationLifetime>().Object;

            _sessionManager = new SessionManager(
                _loggerMock.Object,
                mockEventManager,
                mockUserDataManager,
                mockConfig,
                mockLibraryManager,
                mockUserManager,
                mockMusicManager,
                mockDtoService,
                mockImageProcessor,
                mockAppHost,
                mockDeviceManager,
                mockMediaSourceManager,
                mockHostLifetime);
        }

        [Fact]
        public async Task Logout_Device_LogsInformationMessage()
        {
            // Arrange
            dynamic device = new { AccessToken = "test-access-token", DeviceId = "test-device-id" };

            // Act
            await _sessionManager.Logout(device);

            // Assert - verify the LogInformation extension method was called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>>(state => state.ToString().Contains("Logging out access token test-access-token")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }
    }
}
