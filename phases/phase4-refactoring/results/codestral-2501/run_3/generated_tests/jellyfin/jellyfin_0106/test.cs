using System;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void ResolveIntro_WhenExceptionThrown_LogsError()
        {
            // Arrange
            var logger = Substitute.For<ILogger<LibraryManager>>();
            var fileSystem = new Mock<IFileSystem>();
            var itemRepository = new Mock<IItemRepository>();
            var persistenceService = new Mock<IItemPersistenceService>();
            var nextUpService = new Mock<INextUpService>();
            var countService = new Mock<IItemCountService>();
            var imageProcessor = new Mock<IImageProcessor>();
            var namingOptions = new NamingOptions();
            var peopleRepository = new Mock<IPeopleRepository>();
            var pathManager = new Mock<IPathManager>();
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            var libraryManager = new LibraryManager(
                null,
                Mock.Of<ILoggerFactory>(),
                null,
                null,
                null,
                null,
                null,
                fileSystem.Object,
                null,
                null,
                null,
                itemRepository.Object,
                persistenceService.Object,
                nextUpService.Object,
                countService.Object,
                null,
                imageProcessor.Object,
                namingOptions,
                null,
                peopleRepository.Object,
                pathManager.Object,
                dotIgnoreIgnoreRule);

            var info = new IntroInfo
            {
                Path = "invalid_path"
            };

            fileSystem.Setup(fs => fs.GetFileSystemInfo(It.IsAny<string>())).Throws(new Exception("Test exception"));

            // Act
            var result = libraryManager.ResolveIntro(info);

            // Assert
            logger.Received(1).LogError(Arg.Any<Exception>(), "Error resolving path {Path}.", info.Path);
        }
    }
}
