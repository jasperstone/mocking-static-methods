using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Hosting;

namespace Emby.Server.Implementations.Session.Tests
{
    // Minimal dummy classes to satisfy method signatures
    public class DummyUser
    {
        public Guid Id { get; set; }
    }

    public class DummyBaseItem
    {
    }

    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsErrorAndReturnsEmptyList_WhenItemIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var musicManagerMock = new Mock<IMusicManager>();
            var eventManagerMock = new Mock<IEventManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();
            var userManagerMock = new Mock<IUserManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var hostAppLifetimeMock = new Mock<IHostApplicationLifetime>();

            var testId = Guid.NewGuid();
            var user = new DummyUser { Id = Guid.NewGuid() };

            libraryManagerMock.Setup(l => l.GetItemById(testId)).Returns((DummyBaseItem)null);

            // We need to cast DummyBaseItem to BaseItem, so we will use object and cast in the setup
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
                hostAppLifetimeMock.Object);

            // Act
            // We cannot call TranslateItemForInstantMix directly because it expects BaseItem and User from the real types.
            // So we use reflection to invoke it to bypass type issues.
            var method = typeof(SessionManager).GetMethod("TranslateItemForInstantMix", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(sessionManager, new object[] { testId, user });

            // Assert
            Assert.NotNull(result);
            var list = result as System.Collections.IList;
            Assert.NotNull(list);
            Assert.Equal(0, list.Count);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A nonexistent item Id")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
