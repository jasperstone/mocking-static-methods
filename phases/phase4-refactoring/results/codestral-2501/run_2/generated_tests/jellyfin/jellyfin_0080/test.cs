using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void DeleteMetadataPath_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var libraryManager = new Mock<LibraryManager>(
                null, // IServerApplicationHost
                Mock.Of<ILoggerFactory>(), // ILoggerFactory
                null, // ITaskManager
                null, // IUserManager
                null, // IServerConfigurationManager
                null, // IUserDataManager
                null, // Lazy<ILibraryMonitor>
                null, // IFileSystem
                null, // Lazy<IProviderManager>
                null, // Lazy<IUserViewManager>
                null, // IMediaEncoder
                null, // IItemRepository
                null, // IItemPersistenceService
                null, // INextUpService
                null, // IItemCountService
                null, // ILinkedChildrenService
                null, // IImageProcessor
                null, // NamingOptions
                null, // IDirectoryService
                null, // IPeopleRepository
                null, // IPathManager
                null // DotIgnoreIgnoreRule
            );

            var item = new BaseItem
            {
                Id = Guid.NewGuid(),
                Name = "Test Item",
                IsFolder = false
            };

            var metadataPath = "test/path";

            // Act
            libraryManager.Object.DeleteMetadataPath(item, metadataPath);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
