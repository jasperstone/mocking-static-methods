using System;
using System.Threading;
using System.Threading.Tasks;
using Emby.Photos;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using TagLib;
using Xunit;

namespace Emby.Photos.Tests
{
    public class PhotoProviderTests
    {
        [Fact]
        public async Task FetchAsync_LogsError_WhenTagLibThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PhotoProvider>>();
            var imageProcessorMock = new Mock<IImageProcessor>();

            var photoProvider = new PhotoProvider(loggerMock.Object, imageProcessorMock.Object);

            var photoMock = new Mock<Photo>();
            photoMock.SetupAllProperties();
            photoMock.Object.Path = "test.jpg";
            photoMock.Object.LockedFields = new System.Collections.Generic.HashSet<MetadataField>();

            // Setup photo.Path extension to be in the includeExtensions list
            // We will mock TagLib.File.Create to throw an exception to trigger the catch block

            // We need to mock TagLib.File.Create to throw, but it's a static method.
            // So we will use a wrapper class or partial class to override it.
            // Since we cannot change the original code, we will simulate by passing a path that causes TagLib.File.Create to throw.

            // To simulate this, we will create a derived class that overrides FetchAsync and calls base.FetchAsync but with a path that causes exception.
            // But since we cannot do that easily, we will instead create a fake file path that does not exist, so TagLib.File.Create throws.

            // Act
            var result = await photoProvider.FetchAsync(photoMock.Object, null, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image Provider - Error reading image tag for")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
