using Moq;
using Xunit;
using Jellyfin.LiveTv;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using MediaBrowser.Model.Dto; // For BaseItemDto
using MediaBrowser.Model.Entities; // For ImageInfo
using Jellyfin.Controller; // For IImageProcessor, IApplicationHost, IDtoService

public class LiveTvDtoServiceTests
{
    [Fact]
    public void FillImages_LogsError_WhenExceptionThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LiveTvDtoService>>();
        var imageProcessorMock = new Mock<IImageProcessor>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var appHostMock = new Mock<IApplicationHost>();
        var dtoServiceMock = new Mock<IDtoService>();

        var service = new LiveTvDtoService(dtoServiceMock.Object, imageProcessorMock.Object, loggerMock.Object, appHostMock.Object, libraryManagerMock.Object);

        var dto = new BaseItemDto();
        var seriesName = "Test Series";
        var programSeriesId = "123";

        // Simulate an exception when getting the image cache tag
        imageProcessorMock
            .Setup(x => x.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>()))
            .Throws(new InvalidOperationException("Test Exception")); // Use a more specific exception type

        // Act
        service.FillImages(dto, seriesName, programSeriesId);

        // Assert
        loggerMock.Verify(
            x => x.LogError(It.IsAny<Exception>(), "Error"),
            Times.Exactly(2)); // Two calls to LogError in the FillImages method
    }
}
