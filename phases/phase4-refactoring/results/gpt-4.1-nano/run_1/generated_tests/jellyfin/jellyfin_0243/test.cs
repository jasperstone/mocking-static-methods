using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Session;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Session;
using Emby.Server.Implementations.Session;

namespace Emby.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_NonexistentItem_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var userManagerMock = new Mock<IUserManager>();
            var musicManagerMock = new Mock<IMusicManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var hostLifetimeMock = new Mock<IHostApplicationLifetime>();

            var sessionManager = new SessionManager(
                loggerMock.Object,
                new Mock<IEventManager>().Object,
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
                hostLifetimeMock.Object);

            var testId = Guid.NewGuid();

            // Setup _libraryManager.GetItemById to return null
            var libMock = new Mock<ILibraryManager>();
            libMock.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((IItem)null);
            // Inject the mock into sessionManager via reflection
            var libField = typeof(SessionManager).GetField("_libraryManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            libField.SetValue(sessionManager, libMock.Object);

            // Act
            var result = sessionManager.TranslateItemForInstantMix(testId, new User());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            loggerMock.Verify(
                x => x.LogError("A nonexistent item Id {0} was passed into TranslateItemForInstantMix", testId),
                Times.Once);
        }
    }
}
