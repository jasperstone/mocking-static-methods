using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using jellyfin.Server.Implementations.Trickplay;

namespace Jellyfin.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task CreateTrickplayFiles_ShouldLogInformation_WhenSuccessful()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TrickplayManager>>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockEncodingHelper = new Mock<IEncodingHelper>();
            var mockOutputDir = new Mock<IDirectory>();
            var mockCreateTiles = new Func<string[], int, object, string, object>((images, width, options, outputDir) => new { ItemId = "" });
            var mockSaveTrickplayInfo = new Func<object, Task>(async (info) => { await Task.CompletedTask; });
            var trickplayManager = new TrickplayManager(
                mockLogger.Object,
                mockFileSystem.Object,
                mockEncodingHelper.Object,
                mockOutputDir.Object,
                mockCreateTiles,
                mockSaveTrickplayInfo);

            var options = new TrickplayOptions
            {
                EnableHwAcceleration = true,
                EnableHwEncoding = true,
                ProcessThreads = 4,
                Qscale = 1,
                ProcessPriority = 2,
                EnableKeyFrameOnlyExtraction = false
            };

            var mediaPath = "media/path";
            var video = new { Id = "video1" };
            var outputDir = new DirectoryInfo("output");
            var imgTempDir = "tempDir";

            // Setup mocks
            mockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), false, false))
                .Returns(new[] { new FileInfo("img1"), new FileInfo("img2") });
            mockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), false, false))
                .Returns(new[] { new FileInfo("img1"), new FileInfo("img2") });
            mockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), false, false))
                .Returns(new[] { new FileInfo("img1"), new FileInfo("img2") });
            mockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), false, false))
                .Returns(new[] { new FileInfo("img1"), new FileInfo("img2") });
            // Simulate CreateTiles returning a non-null object
            // For simplicity, assume CreateTiles is a method in TrickplayManager that returns an object
            // and that it is called with images, width, options, outputDir.FullName
            // We will mock it by replacing the method in the test class if needed

            // Act
            await trickplayManager.CreateTrickplayFilesAsync(mediaPath, video, options, outputDir, imgTempDir, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
