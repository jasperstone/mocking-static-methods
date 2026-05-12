using Emby.Media.Processing;
using Emby.Photos;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Photos.Tests
{
    public class PhotoProviderTests
    {
        private readonly Mock<ILogger<PhotoProvider>> _loggerMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly PhotoProvider _provider;

        public PhotoProviderTests()
        {
            _loggerMock = new Mock<ILogger<PhotoProvider>>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _provider = new PhotoProvider(_loggerMock.Object, _imageProcessorMock.Object);
        }

        [Fact]
        public async Task FetchAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var item = new Photo
            {
                Path = "path/to/image.jpg"
            };

            _imageProcessorMock.Setup(p => p.GetImageDimensions(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Test exception"));

            // Act
            await _provider.FetchAsync(item, new MetadataRefreshOptions(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Image Provider - Error reading image tag for {0}", item.Path), Times.Once);
        }

        [Fact]
        public async Task FetchAsync_DoesNotLogError_WhenNoExceptionOccurs()
        {
            // Arrange
            var item = new Photo
            {
                Path = "path/to/image.jpg"
            };

            _imageProcessorMock.Setup(p => p.GetImageDimensions(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Size(100, 100));

            // Act
            await _provider.FetchAsync(item, new MetadataRefreshOptions(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Image Provider - Error reading image tag for {0}", item.Path), Times.Never);
        }
    }
}
