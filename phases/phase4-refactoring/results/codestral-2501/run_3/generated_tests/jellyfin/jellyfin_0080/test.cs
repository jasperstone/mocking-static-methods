using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using System;
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
            var libraryManager = new LibraryManager(
                appHost: null,
                loggerFactory: Mock.Of<ILoggerFactory>(),
                taskManager: null,
                userManager: null,
                configurationManager: null,
                userDataManager: null,
                libraryMonitorFactory: null,
                fileSystem: null,
                providerManagerFactory: null,
                userViewManagerFactory: null,
                mediaEncoder: null,
                itemRepository: null,
                persistenceService: null,
                nextUpService: null,
                countService: null,
                linkedChildrenService: null,
                imageProcessor: null,
                namingOptions: null,
                directoryService: null,
                peopleRepository: null,
                pathManager: null,
                dotIgnoreIgnoreRule: null
            );

            var item = new Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Item"
            };

            var metadataPath = "test/path";

            // Act
            libraryManager.DeleteItem(item, new DeleteOptions());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
