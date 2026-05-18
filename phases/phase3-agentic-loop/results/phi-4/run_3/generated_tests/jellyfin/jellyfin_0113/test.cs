using System;
using System.IO;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using Emby.Server.Implementations.Library.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Tests.Library
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task LogWarning_WhenImageNotFound_CallsLoggerWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var item = new Item { Id = Guid.NewGuid() }; // Use a concrete class
            var img = new Image { Path = "test/path/image.jpg", IsLocalFile = true };

            var libraryManager = new LibraryManager(
                appHost: null, // Mock or provide necessary dependencies
                loggerFactory: new LoggerFactory().AddProvider(new MockLoggerProvider(loggerMock.Object)), // Provide logger factory
                taskManager: null, // Mock or provide necessary dependencies
                userManager: null, // Mock or provide necessary dependencies
                configurationManager: null, // Mock or provide necessary dependencies
                userDataManager: null, // Mock or provide necessary dependencies
                libraryMonitorFactory: null, // Mock or provide necessary dependencies
                fileSystem: new MockFileSystem(), // Mock file system
                providerManagerFactory: null, // Mock or provide necessary dependencies
                userViewManagerFactory: null, // Mock or provide necessary dependencies
                mediaEncoder: null, // Mock or provide necessary dependencies
                itemRepository: null, // Mock or provide necessary dependencies
                persistenceService: null, // Mock or provide necessary dependencies
                nextUpService: null, // Mock or provide necessary dependencies
                countService: null, // Mock or provide necessary dependencies
                linkedChildrenService: null, // Mock or provide necessary dependencies
                imageProcessor: imageProcessorMock.Object,
                namingOptions: null, // Mock or provide necessary dependencies
                directoryService: null, // Mock or provide necessary dependencies
                peopleRepository: null, // Mock or provide necessary dependencies
                pathManager: null, // Mock or provide necessary dependencies
                dotIgnoreIgnoreRule: null // Mock or provide necessary dependencies
            );

            // Act
            await libraryManager.ProcessImagesAsync(item, new[] { img }); // Use a public method

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "Image not found at {ImagePath}", img.Path),
                Times.Once);
        }
    }
}
