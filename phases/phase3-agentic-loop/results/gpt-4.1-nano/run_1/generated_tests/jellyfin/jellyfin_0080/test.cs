using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;

namespace Emby.Tests.Library
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            // Mock dependencies as needed, for this test only logger is used
            _libraryManager = new LibraryManager(
                Mock.Of<Emby.Server.Implementations.IServerApplicationHost>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
                Mock.Of<Emby.Server.Implementations.ITaskManager>(),
                Mock.Of<Emby.Server.Implementations.IUserManager>(),
                Mock.Of<Emby.Server.Implementations.IServerConfigurationManager>(),
                Mock.Of<Emby.Server.Implementations.IUserDataManager>(),
                new Lazy<Emby.Server.Implementations.Library.ILibraryMonitor>(() => Mock.Of<Emby.Server.Implementations.Library.ILibraryMonitor>()),
                Mock.Of<Emby.Server.Implementations.IFileSystem>(),
                new Lazy<Emby.Server.Implementations.Library.IProviderManager>(() => Mock.Of<Emby.Server.Implementations.Library.IProviderManager>()),
                new Lazy<Emby.Server.Implementations.Library.IUserViewManager>(() => Mock.Of<Emby.Server.Implementations.Library.IUserViewManager>()),
                Mock.Of<Emby.Server.Implementations.IMediaEncoder>(),
                Mock.Of<Emby.Server.Implementations.IItemRepository>(),
                Mock.Of<Emby.Server.Implementations.IItemPersistenceService>(),
                Mock.Of<Emby.Server.Implementations.INextUpService>(),
                Mock.Of<Emby.Server.Implementations.IItemCountService>(),
                Mock.Of<Emby.Server.Implementations.ILinkedChildrenService>(),
                Mock.Of<Emby.Server.Implementations.IImageProcessor>(),
                new Emby.Server.Implementations.Library.NamingOptions(),
                Mock.Of<Emby.Server.Implementations.IDirectoryService>(),
                Mock.Of<Emby.Server.Implementations.IPeopleRepository>(),
                Mock.Of<Emby.Server.Implementations.IPathManager>(),
                new Emby.Server.Implementations.Library.DotIgnoreIgnoreRule()
            );
        }

        [Fact]
        public void LogDebug_IsCalled_WhenDeletingMetadataPath()
        {
            // Arrange
            var itemMock = new Mock<BaseItem>();
            itemMock.Setup(i => i.GetType()).Returns(typeof(Video));
            itemMock.Setup(i => i.Name).Returns("TestVideo");
            itemMock.Setup(i => i.Id).Returns(Guid.NewGuid());
            itemMock.Setup(i => i.IsFolder).Returns(false);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.GetDeletePaths()).Returns(new List<string> { "path1", "path2" });

            // Use reflection to invoke the private method containing LogDebug
            var methodInfo = typeof(LibraryManager).GetMethod("DeleteMetadataPaths", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            methodInfo.Invoke(_libraryManager, new object[] { itemMock.Object, new List<string> { "path1", "path2" } });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting metadata path")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
