using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ConvertImageToLocal_LogsDebugOnHttpRequestException()
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

            var item = new BaseItem();
            var image = new ItemImageInfo { Path = "http://example.com/image.jpg" };
            var exception = new HttpRequestException
            {
                StatusCode = HttpStatusCode.NotFound
            };

            providerManagerMock
                .Setup(p => p.SaveImage(It.IsAny<BaseItem>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => libraryManager.ConvertImageToLocal(item, image, 0, true));

            // Assert
            loggerMock.Verify(
                l => l.LogDebug(
                    It.Is<string>(s => s.Contains("Error downloading image")),
                    It.Is<HttpRequestException>(ex => ex == exception),
                    It.Is<object[]>(objects => objects.Length == 1 && objects[0].ToString() == "http://example.com/image.jpg")),
                Times.Once);
        }
    }
}
