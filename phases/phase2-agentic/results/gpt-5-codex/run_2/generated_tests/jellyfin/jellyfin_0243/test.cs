using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Emby.Server.Implementations.Session;
using MediaBrowser.Common.Events;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_WhenItemIsMissing_LogsErrorAndReturnsEmptyList()
        {
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var eventManagerMock = new Mock<IEventManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var userManagerMock = new Mock<IUserManager>();
            var musicManagerMock = new Mock<IMusicManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var deviceManagerMock = new Mock<IDeviceManager>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
            hostApplicationLifetimeMock.SetupGet(x => x.ApplicationStopping).Returns(CancellationToken.None);

            var missingId = Guid.NewGuid();
            libraryManagerMock.Setup(x => x.GetItemById(missingId)).Returns((BaseItem)null);

            var sessionManager = new SessionManager(
                loggerMock.Object,
                eventManagerMock.Object,
                userDataManagerMock.Object,
                serverConfigurationManagerMock.Object,
                libraryManagerMock.Object,
                userManagerMock.Object,
                musicManagerMock.Object,
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                appHostMock.Object,
                deviceManagerMock.Object,
                mediaSourceManagerMock.Object,
                hostApplicationLifetimeMock.Object);

            var translateItemMethod = typeof(SessionManager).GetMethod(
                "TranslateItemForInstantMix",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(translateItemMethod);

            var result = translateItemMethod!.Invoke(sessionManager, new object[] { missingId, null });

            var items = Assert.IsType<List<BaseItem>>(result);
            Assert.Empty(items);

            var logInvocation = Assert.Single(loggerMock.Invocations.Where(invocation =>
                invocation.Method.Name == nameof(ILogger.Log)));

            Assert.Equal(LogLevel.Error, (LogLevel)logInvocation.Arguments[0]);
            Assert.Equal(
                $"A nonexistent item Id {missingId} was passed into TranslateItemForInstantMix",
                logInvocation.Arguments[2].ToString());
            Assert.Null(logInvocation.Arguments[3]);

            musicManagerMock.Verify(
                x => x.GetInstantMixFromItem(It.IsAny<BaseItem>(), It.IsAny<User>(), It.IsAny<DtoOptions>()),
                Times.Never);

            libraryManagerMock.Verify(x => x.GetItemById(missingId), Times.Once);
        }
    }
}
