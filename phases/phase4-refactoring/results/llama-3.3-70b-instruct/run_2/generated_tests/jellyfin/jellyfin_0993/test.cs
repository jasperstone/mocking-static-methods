using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        private readonly Mock<ILogger<ImageProcessor>> _loggerMock;

        public ImageProcessorTests()
        {
            _loggerMock = new Mock<ILogger<ImageProcessor>>();
        }

        [Fact]
        public void GetImageDimensions_LogsDebugMessage()
        {
            // Arrange
            var item = new User { }; // Use a concrete class that inherits from BaseItem
            var info = new ItemImageInfo { Path = "path" };
            var imageProcessor = new ImageProcessor(_loggerMock.Object, null, null, null, null);

            // Act
            imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(logger => logger.LogDebug("Getting image size for item {ItemType} {Path}", item.GetType().Name, info.Path), Times.Once);
        }
    }
}
