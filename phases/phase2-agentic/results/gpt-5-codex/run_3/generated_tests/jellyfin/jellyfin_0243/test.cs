using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task TranslateItemForInstantMix_LogsErrorWhenItemIsMissing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var missingItemId = Guid.NewGuid();
            libraryManagerMock.Setup(m => m.GetItemById(missingItemId)).Returns((BaseItem)null);

            var hostAppLifetimeMock = new Mock<IHostApplicationLifetime>();
            var cts = new CancellationTokenSource();
            hostAppLifetimeMock.SetupGet(l => l.ApplicationStopping).Returns(cts.Token);
            hostAppLifetimeMock.SetupGet(l => l.ApplicationStopped).Returns(CancellationToken.None);
            hostAppLifetimeMock.SetupGet(l => l.ApplicationStarted).Returns(CancellationToken.None);

            await using var sessionManager = new SessionManager(
                loggerMock.Object,
                new Mock<IEventManager>().Object,
                new Mock<IUserDataManager>().Object,
                new Mock<IServerConfigurationManager>().Object,
                libraryManagerMock.Object,
                new Mock<IUserManager>().Object,
                new Mock<IMusicManager>().Object,
                new Mock<IDtoService>().Object,
                new Mock<IImageProcessor>().Object,
                new Mock<IServerApplicationHost>().Object,
                new Mock<IDeviceManager>().Object,
                new Mock<IMediaSourceManager>().Object,
                hostAppLifetimeMock.Object);

            var translateMethod = typeof(SessionManager).GetMethod("TranslateItemForInstantMix", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(translateMethod);

            // Act
            var result = translateMethod!.Invoke(sessionManager, new object[] { missingItemId, null });
            var items = Assert.IsType<List<BaseItem>>(result);

            // Assert
            Assert.Empty(items);
            loggerMock.Verify(logger => logger.LogError("A nonexistent item Id {0} was passed into TranslateItemForInstantMix", missingItemId), Times.Once);
        }
    }
}
