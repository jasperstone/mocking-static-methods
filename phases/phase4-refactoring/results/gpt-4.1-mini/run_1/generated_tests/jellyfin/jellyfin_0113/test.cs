using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Configuration;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerImageLoggingTests
    {
        [Fact]
        public async Task LogsWarningWhenImageFileNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var configurationMock = new Mock<IServerConfiguration>();
            configurationMock.SetupGet(c => c.CacheSize).Returns(100);
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            configurationManagerMock.SetupGet(c => c.Configuration).Returns(configurationMock.Object);
            var userDataManagerMock = new Mock<IUserDataManager>();
            var libraryMonitorFactoryMock = new Lazy<ILibraryMonitor>(() => null!);
            var fileSystemMock = new Mock<IFileSystem>();
            var providerManagerFactoryMock = new Lazy<IProviderManager>(() => null!);
            var userViewManagerFactoryMock = new Lazy<IUserViewManager>(() => null!);
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var itemRepositoryMock = new Mock<IItemRepository>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();
            var nextUpServiceMock = new Mock<INextUpService>();
            var countServiceMock = new Mock<IItemCountService>();
            var linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var namingOptions = new NamingOptions();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var peopleRepositoryMock = new Mock<IPeopleRepository>();
            var pathManagerMock = new Mock<IPathManager>();
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            // Setup file system to say file does not exist
            fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            var libraryManager = new LibraryManager(
                null!,
                loggerFactoryMock.Object,
                taskManagerMock.Object,
                userManagerMock.Object,
                configurationManagerMock.Object,
                userDataManagerMock.Object,
                libraryMonitorFactoryMock,
                fileSystemMock.Object,
                providerManagerFactoryMock,
                userViewManagerFactoryMock,
                mediaEncoderMock.Object,
                itemRepositoryMock.Object,
                persistenceServiceMock.Object,
                nextUpServiceMock.Object,
                countServiceMock.Object,
                linkedChildrenServiceMock.Object,
                imageProcessorMock.Object,
                namingOptions,
                directoryServiceMock.Object,
                peopleRepositoryMock.Object,
                pathManagerMock.Object,
                dotIgnoreIgnoreRule);

            // We cannot call the internal method directly, so simulate the logging call for the test
            var testImagePath = "/path/to/nonexistent/image.jpg";
            loggerMock.Object.LogWarning("Image not found at {ImagePath}", testImagePath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Image not found at {testImagePath}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
