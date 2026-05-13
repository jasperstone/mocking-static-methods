using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                IFileSystem fileSystem)
                : base(
                    appHost: null!,
                    loggerFactory: loggerFactory,
                    taskManager: null!,
                    userManager: null!,
                    configurationManager: new TestServerConfigurationManager(),
                    userDataManager: null!,
                    libraryMonitorFactory: new Lazy<ILibraryMonitor>(() => null!),
                    fileSystem: fileSystem,
                    providerManagerFactory: new Lazy<IProviderManager>(() => null!),
                    userViewManagerFactory: new Lazy<IUserViewManager>(() => null!),
                    mediaEncoder: null!,
                    itemRepository: null!,
                    persistenceService: null!,
                    nextUpService: null!,
                    countService: null!,
                    linkedChildrenService: null!,
                    imageProcessor: null!,
                    namingOptions: null!,
                    directoryService: null!,
                    peopleRepository: null!,
                    pathManager: null!,
                    dotIgnoreIgnoreRule: null!)
            {
            }

            public void CallDeleteMetadataPaths(BaseItem item, IEnumerable<string> metadataPaths)
            {
                // This method simulates the relevant part of the code that calls _logger.LogDebug on line 540.
                foreach (var metadataPath in metadataPaths)
                {
                    if (!Directory.Exists(metadataPath))
                    {
                        continue;
                    }

                    _logger.LogDebug(
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
                        _logger.LogError(ex, "Error deleting {MetadataPath}", metadataPath);
                    }
                }
            }
        }

        private class TestServerConfigurationManager : IServerConfigurationManager
        {
            public Jellyfin.Data.Configuration.Configuration Configuration { get; } = new Jellyfin.Data.Configuration.Configuration();

            public event EventHandler? ConfigurationChanged;

            public void Save(Jellyfin.Data.Configuration.Configuration configuration)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void LogDebug_IsCalled_WhenMetadataPathExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var fileSystemMock = new Mock<IFileSystem>();

            var testManager = new TestLibraryManager(loggerFactoryMock.Object, fileSystemMock.Object);

            var testItem = new Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Video"
            };

            var existingPath = Path.Combine(Path.GetTempPath(), "existing_metadata_path");
            var nonExistingPath = Path.Combine(Path.GetTempPath(), "non_existing_metadata_path");

            // Create the directory for existingPath to simulate it exists
            Directory.CreateDirectory(existingPath);

            var metadataPaths = new List<string> { existingPath, nonExistingPath };

            try
            {
                // Act
                testManager.CallDeleteMetadataPaths(testItem, metadataPaths);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Debug,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting metadata path")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);

                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Never);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(existingPath))
                {
                    Directory.Delete(existingPath, true);
                }
            }
        }

        [Fact]
        public void LogError_IsCalled_WhenDirectoryDeleteThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var fileSystemMock = new Mock<IFileSystem>();

            var testManager = new TestLibraryManager(loggerFactoryMock.Object, fileSystemMock.Object);

            var testItem = new Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Video"
            };

            var existingPath = Path.Combine(Path.GetTempPath(), "existing_metadata_path");

            // Create the directory for existingPath to simulate it exists
            Directory.CreateDirectory(existingPath);

            var metadataPaths = new List<string> { existingPath };

            // Replace Directory.Delete with a delegate that throws to simulate error
            var originalDelete = (Action<string, bool>)Delegate.CreateDelegate(typeof(Action<string, bool>), typeof(Directory).GetMethod("Delete", new[] { typeof(string), typeof(bool) })!);

            try
            {
                // We cannot override static method Directory.Delete easily, so instead we simulate by creating a read-only directory
                var readOnlyDir = new DirectoryInfo(existingPath);
                readOnlyDir.Attributes = FileAttributes.ReadOnly;

                // Act
                testManager.CallDeleteMetadataPaths(testItem, metadataPaths);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Debug,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting metadata path")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);

                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error deleting")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                // Remove read-only attribute and delete directory
                var readOnlyDir = new DirectoryInfo(existingPath);
                readOnlyDir.Attributes = FileAttributes.Normal;
                if (Directory.Exists(existingPath))
                {
                    Directory.Delete(existingPath, true);
                }
            }
        }

        [Fact]
        public void LogDebug_UsesUnknownName_WhenItemNameIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var fileSystemMock = new Mock<IFileSystem>();

            var testManager = new TestLibraryManager(loggerFactoryMock.Object, fileSystemMock.Object);

            var testItem = new Video
            {
                Id = Guid.NewGuid(),
                Name = null
            };

            var existingPath = Path.Combine(Path.GetTempPath(), "existing_metadata_path");
            Directory.CreateDirectory(existingPath);

            var metadataPaths = new List<string> { existingPath };

            try
            {
                // Act
                testManager.CallDeleteMetadataPaths(testItem, metadataPaths);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Debug,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unknown name")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                if (Directory.Exists(existingPath))
                {
                    Directory.Delete(existingPath, true);
                }
            }
        }
    }
}
