using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.Trickplay;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        private class StubVideo
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        private class StubLibraryOptions
        {
            public bool EnableTrickplayImageExtraction { get; set; }
            public bool SaveTrickplayWithMedia { get; set; }
        }

        [Fact]
        public async Task MoveGeneratedTrickplayDataAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var mediaEncoderMock = new Mock<object>();
            var fileSystemMock = new Mock<object>();
            var encodingHelperMock = new Mock<object>();
            var configMock = new Mock<object>();
            var imageEncoderMock = new Mock<object>();
            var dbContextFactoryMock = new Mock<object>();
            var appPathsMock = new Mock<object>();
            var pathManagerMock = new Mock<object>();

            var trickplayManager = new TrickplayManager(
                loggerMock.Object,
                (IMediaEncoder)mediaEncoderMock.Object,
                (IFileSystem)fileSystemMock.Object,
                (EncodingHelper)encodingHelperMock.Object,
                (IServerConfigurationManager)configMock.Object,
                (IImageEncoder)imageEncoderMock.Object,
                (IDbContextFactory<JellyfinDbContext>)dbContextFactoryMock.Object,
                (IApplicationPaths)appPathsMock.Object,
                (IPathManager)pathManagerMock.Object);

            var video = new StubVideo { Id = Guid.NewGuid(), Name = "TestVideo" };
            var libraryOptions = new StubLibraryOptions
            {
                EnableTrickplayImageExtraction = true,
                SaveTrickplayWithMedia = true
            };

            // Act
            await trickplayManager.MoveGeneratedTrickplayDataAsync(
                (MediaBrowser.Model.Entities.Video)(object)video,
                (MediaBrowser.Model.Configuration.LibraryOptions)(object)libraryOptions,
                CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Moved trickplay images for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
