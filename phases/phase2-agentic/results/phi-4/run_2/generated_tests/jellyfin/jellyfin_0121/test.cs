using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
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
        public async Task ConvertImageToLocal_LogsDebugMessageOnImageConversion()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var providerManagerMock = new Mock<IProviderManager>();
            var itemRepositoryMock = new Mock<IItemRepository>();
            var fileSystemMock = new Mock<IFileSystem>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var peopleRepositoryMock = new Mock<IPeopleRepository>();

            var libraryManager = new LibraryManager(
                null, // appHost
                null, // loggerFactory
                null, // taskManager
                null, // userManager
                configurationManagerMock.Object,
                null, // userDataManager
                null, // libraryMonitorFactory
                fileSystemMock.Object,
                new Lazy<IProviderManager>(() => providerManagerMock.Object),
                null, // userViewManagerFactory
                null, // mediaEncoder
                itemRepositoryMock.Object,
                null, // persistenceService
                null, // nextUpService
                null, // countService
                null, // linkedChildrenService
                imageProcessorMock.Object,
                null, // namingOptions
                null, // directoryService
                peopleRepositoryMock.Object,
                null, // pathManager
                null  // dotIgnoreIgnoreRule
            );

            libraryManager._logger = loggerMock.Object;

            var item = new BaseItem { Id = Guid.NewGuid() };
            var image = new ItemImageInfo { Path = "http://example.com/image.jpg" };

            // Act
            await libraryManager.ConvertImageToLocal(item, image, 0, false);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    It.Is<string>(s => s.Contains("ConvertImageToLocal item {0} - image url: {1}")),
                    It.Is<object[]>(o => o[0] == item.Id && o[1] == "http://example.com/image.jpg"),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }
}
