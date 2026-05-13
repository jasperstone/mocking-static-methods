using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Emby.Server.Implementations.Session;
using MediaBrowser.Common.Events;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Events.Session;
using MediaBrowser.Model.SyncPlay;
using MediaBrowser.Model.Session;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Session
{
    public class SessionManagerTests
    {
        [Fact]
        public void TranslateItemForInstantMix_LogsErrorWhenItemNotFound()
        {
            var loggerMock = new Mock<ILogger<SessionManager>>();
            var eventManagerMock = new Mock<IEventManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var userManagerMock = new Mock<IUserManager>();
            var musicManagerMock = new Mock<IMusicManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var deviceManagerMock = new Mock<IDeviceManager>();

            deviceManagerMock
                .SetupAdd(d => d.DeviceOptionsUpdated += It.IsAny<EventHandler<GenericEventArgs<Tuple<string, DeviceOptions>>>>());
            deviceManagerMock
                .SetupRemove(d => d.DeviceOptionsUpdated -= It.IsAny<EventHandler<GenericEventArgs<Tuple<string, DeviceOptions>>>>());

            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var lifetimeMock = new Mock<IHostApplicationLifetime>();
            lifetimeMock.SetupGet(l => l.ApplicationStopping).Returns(CancellationToken.None);

            var sessionManager = new SessionManager(
                loggerMock.Object,
                eventManagerMock.Object,
                userDataManagerMock.Object,
                configManagerMock.Object,
                libraryManagerMock.Object,
                userManagerMock.Object,
                musicManagerMock.Object,
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                appHostMock.Object,
                deviceManagerMock.Object,
                mediaSourceManagerMock.Object,
                lifetimeMock.Object);

            var methodInfo = typeof(SessionManager).GetMethod("TranslateItemForInstantMix", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(methodInfo);

            var itemId = Guid.NewGuid();
            libraryManagerMock.Setup(l => l.GetItemById(itemId)).Returns((BaseItem)null);

            var result = (List<BaseItem>)methodInfo!.Invoke(sessionManager, new object[] { itemId, null });

            Assert.NotNull(result);
            Assert.Empty(result);
            libraryManagerMock.Verify(l => l.GetItemById(itemId), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"A nonexistent item Id {itemId} was passed into TranslateItemForInstantMix"),
                    It.Is<Exception>(ex => ex == null),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
