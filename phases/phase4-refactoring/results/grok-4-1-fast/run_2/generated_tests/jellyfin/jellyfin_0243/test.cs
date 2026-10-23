#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILogger<SessionManager>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IMusicManager> _musicManagerMock;

        public SessionManagerTests()
        {
            _loggerMock = new Mock<ILogger<SessionManager>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _musicManagerMock = new Mock<IMusicManager>();
        }

        [Fact]
        public void TranslateItemForInstantMix_NonexistentItemId_LogsError()
        {
            // Arrange
            var nonexistentId = Guid.NewGuid();
            _libraryManagerMock
                .Setup(lm => lm.GetItemById(nonexistentId))
                .Returns((BaseItem?)null);

            var sessionManager = CreateSessionManager();

            // Act
            var result = InvokeTranslateItemForInstantMix(sessionManager, nonexistentId, null!);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("A nonexistent item Id") && v.ToString()!.Contains(nonexistentId.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void TranslateItemForInstantMix_ValidItem_ReturnsInstantMix()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var mockItem = new Mock<BaseItem>();
            mockItem.Setup(i => i.Id).Returns(itemId);
            var user = new Mock<MediaBrowser.Controller.Entities.User>().Object;
            var expectedItems = new List<BaseItem> { new Mock<BaseItem>().Object };

            _libraryManagerMock
                .Setup(lm => lm.GetItemById(itemId))
                .Returns(mockItem.Object);

            _musicManagerMock
                .Setup(mm => mm.GetInstantMixFromItem(
                    It.IsAny<BaseItem>(),
                    It.IsAny<MediaBrowser.Controller.Entities.User>(),
                    It.Is<DtoOptions>(o => !o.EnableImages)))
                .Returns(expectedItems);

            var sessionManager = CreateSessionManager();

            // Act
            var result = InvokeTranslateItemForInstantMix(sessionManager, itemId, user);

            // Assert
            _loggerMock.VerifyNoOtherCalls();
            Assert.Equal(expectedItems.Count, result!.Count);
        }

        private static List<BaseItem> InvokeTranslateItemForInstantMix(SessionManager sessionManager, Guid id, MediaBrowser.Controller.Entities.User user)
        {
            var method = typeof(SessionManager).GetMethod("TranslateItemForInstantMix", BindingFlags.NonPublic | BindingFlags.Instance)!;
            return (List<BaseItem>)method.Invoke(sessionManager, new object[] { id, user })!;
        }

        private SessionManager CreateSessionManager()
        {
            var mocks = new
            {
                EventManager = new Mock<IEventManager>().Object,
                UserDataManager = new Mock<IUserDataManager>().Object,
                Config = new Mock<IServerConfigurationManager>().Object,
                UserManager = new Mock<IUserManager>().Object,
                DtoService = new Mock<IDtoService>().Object,
                ImageProcessor = new Mock<IImageProcessor>().Object,
                AppHost = new Mock<IServerApplicationHost>().Object,
                DeviceManager = new Mock<IDeviceManager>().Object,
                MediaSourceManager = new Mock<IMediaSourceManager>().Object,
                HostApplicationLifetime = new Mock<IHostApplicationLifetime>().Object
            };

            return new SessionManager(
                _loggerMock.Object,
                mocks.EventManager,
                mocks.UserDataManager,
                mocks.Config,
                _libraryManagerMock.Object,
                mocks.UserManager,
                _musicManagerMock.Object,
                mocks.DtoService,
                mocks.ImageProcessor,
                mocks.AppHost,
                mocks.DeviceManager,
                mocks.MediaSourceManager,
                mocks.HostApplicationLifetime);
        }
    }
}
