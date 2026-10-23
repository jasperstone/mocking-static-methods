using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private class TestFolder : BaseItem
        {
            public override bool IsFolder => true;
            public override string Name { get; set; }
        }

        private class LibraryDeleteOptions : ILibraryDeleteOptions
        {
            public bool DeleteFileLocation { get; set; }
        }

        [Fact]
        public void DeleteItem_LogsDebug_WhenMetadataPathExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var directoryServiceMock = new Mock<IDirectoryService>();
            directoryServiceMock.Setup(d => d.Exists(It.IsAny<string>())).Returns(true);
            directoryServiceMock.Setup(d => d.Delete(It.IsAny<string>(), true));

            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
            fileSystemMock.Setup(f => f.DirectoryDelete(It.IsAny<string>(), true));

            var configurationMock = new Mock<IServerConfigurationManager>();
            configurationMock.SetupGet(c => c.Configuration).Returns(new Configuration { CacheSize = 100 });

            var libraryManager = new LibraryManager(
                appHost: null,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: null,
                userManager: null,
                configurationManager: configurationMock.Object,
                userDataManager: null,
                libraryMonitorFactory: new Lazy<ILibraryMonitor>(() => null),
                fileSystem: fileSystemMock.Object,
                providerManagerFactory: new Lazy<IProviderManager>(() => null),
                userViewManagerFactory: new Lazy<IUserViewManager>(() => null),
                mediaEncoder: null,
                itemRepository: null,
                persistenceService: null,
                nextUpService: null,
                countService: null,
                linkedChildrenService: null,
                imageProcessor: null,
                namingOptions: null,
                directoryService: directoryServiceMock.Object,
                peopleRepository: null,
                pathManager: null,
                dotIgnoreIgnoreRule: null);

            var testFolder = new TestFolder
            {
                Id = Guid.NewGuid(),
                Name = "TestFolder"
            };

            // Act
            libraryManager.DeleteItem(testFolder, new LibraryDeleteOptions { DeleteFileLocation = false });

            // Assert
            loggerMock.Verify(
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
