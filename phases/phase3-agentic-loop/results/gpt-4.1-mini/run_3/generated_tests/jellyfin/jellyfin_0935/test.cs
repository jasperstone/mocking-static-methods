using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        private class TestVideo : BaseItem
        {
            public override string Name { get; set; }
        }

        private class TestLibraryOptions { }

        [Fact]
        public async Task ExecuteAsync_LogsErrorWhenRefreshTrickplayDataAsyncThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var localizationMock = new Mock<MediaBrowser.Model.Globalization.ILocalizationManager>();
            var trickplayManagerMock = new Mock<ITrickplayManager>();

            var video = new TestVideo { Name = "TestVideo" };

            libraryManagerMock.Setup(lm => lm.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem> { video });
            libraryManagerMock.Setup(lm => lm.GetLibraryOptions(video)).Returns(new TestLibraryOptions());

            trickplayManagerMock
                .Setup(tm => tm.RefreshTrickplayDataAsync(video, false, It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var task = new TrickplayImagesTask(
                loggerMock.Object,
                libraryManagerMock.Object,
                localizationMock.Object,
                trickplayManagerMock.Object);

            var progressReports = new List<double>();
            var progress = new Progress<double>(p => progressReports.Add(p));
            var cancellationToken = CancellationToken.None;

            // Act
            await task.ExecuteAsync(progress, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating trickplay files for TestVideo")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Contains(100, progressReports);
        }
    }
}
