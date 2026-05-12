using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Model.Drawing;
using TagLib;

namespace Emby.Photos.Tests
{
    public class PhotoProviderTests
    {
        private readonly Mock<ILogger<PhotoProvider>> _mockLogger;
        private readonly Mock<IImageProcessor> _mockImageProcessor;
        private readonly PhotoProvider _photoProvider;

        public PhotoProviderTests()
        {
            _mockLogger = new Mock<ILogger<PhotoProvider>>();
            _mockImageProcessor = new Mock<IImageProcessor>();
            _photoProvider = new PhotoProvider(_mockLogger.Object, _mockImageProcessor.Object);
        }

        [Fact]
        public async Task FetchAsync_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var item = new Photo { Path = "test.jpg" };
            var options = new MetadataRefreshOptions();
            var cancellationToken = CancellationToken.None;

            _mockLogger.Setup(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()))
                      .Verifiable();

            // Act
            await _photoProvider.FetchAsync(item, options, cancellationToken);

            // Assert
            _mockLogger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task FetchAsync_ShouldSetImageDimensions_WhenWidthOrHeightIsZero()
        {
            // Arrange
            var item = new Photo { Path = "test.jpg", Width = 0, Height = 0 };
            var options = new MetadataRefreshOptions();
            var cancellationToken = CancellationToken.None;
            var imageInfo = new ImageInfo { Path = "test.jpg" };
            var size = new ImageDimensions { Width = 100, Height = 200 };

            _mockImageProcessor.Setup(x => x.GetImageDimensions(item, imageInfo)).Returns(size);

            // Act
            await _photoProvider.FetchAsync(item, options, cancellationToken);

            // Assert
            Assert.Equal(100, item.Width);
            Assert.Equal(200, item.Height);
        }

        [Fact]
        public async Task FetchAsync_ShouldNotSetImageDimensions_WhenExceptionOccurs()
        {
            // Arrange
            var item = new Photo { Path = "test.jpg", Width = 0, Height = 0 };
            var options = new MetadataRefreshOptions();
            var cancellationToken = CancellationToken.None;
            var imageInfo = new ImageInfo { Path = "test.jpg" };

            _mockImageProcessor.Setup(x => x.GetImageDimensions(item, imageInfo)).Throws<ArgumentException>();

            // Act
            await _photoProvider.FetchAsync(item, options, cancellationToken);

            // Assert
            Assert.Equal(0, item.Width);
            Assert.Equal(0, item.Height);
        }
    }
}
