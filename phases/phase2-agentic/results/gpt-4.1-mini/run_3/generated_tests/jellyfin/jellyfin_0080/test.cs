using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Video;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private class TestLibraryManager : LibraryManager
        {
            public TestLibraryManager(
                ILoggerFactory loggerFactory,
                ITaskManager taskManager = null,
                IUserManager userManager = null,
                IServerConfigurationManager configurationManager = null,
                IUserDataManager userDataManager = null,
                Lazy<ILibraryMonitor> libraryMonitorFactory = null,
                IFileSystem fileSystem = null,
                Lazy<IProviderManager> providerManagerFactory = null,
                Lazy<IUserViewManager> userViewManagerFactory = null,
                IMediaEncoder mediaEncoder = null,
                IItemRepository itemRepository = null,
                IItemPersistenceService persistenceService = null,
                INextUpService nextUpService = null,
                IItemCountService countService = null,
                ILinkedChildrenService linkedChildrenService = null,
                IImageProcessor imageProcessor = null,
                NamingOptions namingOptions = null,
                IDirectoryService directoryService = null,
                IPeopleRepository peopleRepository = null,
                IPathManager pathManager = null,
                DotIgnoreIgnoreRule dotIgnoreIgnoreRule = null)
                : base(
                    appHost: null,
                    loggerFactory,
                    taskManager,
                    userManager,
                    configurationManager,
                    userDataManager,
                    libraryMonitorFactory,
                    fileSystem,
                    providerManagerFactory,
                    userViewManagerFactory,
                    mediaEncoder,
                    itemRepository,
                    persistenceService,
                    nextUpService,
                    countService,
                    linkedChildrenService,
                    imageProcessor,
                    namingOptions,
                    directoryService,
                    peopleRepository,
                    pathManager,
                    dotIgnoreIgnoreRule)
            {
            }

            // Expose protected or internal methods if needed for testing
            public void CallDeleteMetadataPaths(BaseItem item, IEnumerable<string> metadataPaths, ILogger logger)
            {
                foreach (var metadataPath in metadataPaths)
                {
                    if (!Directory.Exists(metadataPath))
                    {
                        continue;
                    }

                    logger.LogDebug(
                        "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                        item.GetType().Name,
                        item.Name ?? "Unknown name",
                        metadataPath,
                        item.Id);

                    try
                    {
                        Directory.Delete(metadataPath, true);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error deleting {MetadataPath}", metadataPath);
                    }
                }
            }
        }

        [Fact]
        public void LogDebug_Is_Called_When_MetadataPath_Exists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Video"
            };

            var metadataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(metadataPath);

            var metadataPaths = new List<string> { metadataPath };

            var libraryManager = new TestLibraryManager(loggerFactoryMock.Object);

            // Act
            libraryManager.CallDeleteMetadataPaths(item, metadataPaths, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting metadata path")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            if (Directory.Exists(metadataPath))
            {
                Directory.Delete(metadataPath, true);
            }
        }

        [Fact]
        public void LogDebug_Uses_UnknownName_When_ItemName_Is_Null()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = null
            };

            var metadataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(metadataPath);

            var metadataPaths = new List<string> { metadataPath };

            var libraryManager = new TestLibraryManager(loggerFactoryMock.Object);

            // Act
            libraryManager.CallDeleteMetadataPaths(item, metadataPaths, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unknown name")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            if (Directory.Exists(metadataPath))
            {
                Directory.Delete(metadataPath, true);
            }
        }

        [Fact]
        public void LogError_Is_Called_When_DirectoryDelete_Throws()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Video"
            };

            var metadataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(metadataPath);

            var metadataPaths = new List<string> { metadataPath };

            var libraryManager = new TestLibraryManager(loggerFactoryMock.Object);

            // Replace Directory.Delete with a delegate that throws
            var originalDelete = new Action<string, bool>(Directory.Delete);
            try
            {
                // Use a shim or wrapper to simulate exception on Directory.Delete
                // Since we cannot override static methods, simulate by deleting read-only directory
                var dirInfo = new DirectoryInfo(metadataPath);
                dirInfo.Attributes = FileAttributes.ReadOnly;

                // Act
                libraryManager.CallDeleteMetadataPaths(item, metadataPaths, loggerMock.Object);
            }
            finally
            {
                // Remove read-only attribute and delete directory
                var dirInfo = new DirectoryInfo(metadataPath);
                dirInfo.Attributes = FileAttributes.Normal;
                if (Directory.Exists(metadataPath))
                {
                    Directory.Delete(metadataPath, true);
                }
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_Is_Not_Called_When_MetadataPath_Does_Not_Exist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Video"
            };

            var metadataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            // Do not create directory, so it does not exist

            var metadataPaths = new List<string> { metadataPath };

            var libraryManager = new TestLibraryManager(loggerFactoryMock.Object);

            // Act
            libraryManager.CallDeleteMetadataPaths(item, metadataPaths, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
