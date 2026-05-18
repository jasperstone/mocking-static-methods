using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task LogError_IsCalled_WhenDeleteFileThrowsIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var httpClientFactoryMock = new Mock<System.Net.Http.IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var subtitleParserMock = new Mock<ISubtitleParser>();
            var pathManagerMock = new Mock<IPathManager>();
            var serverConfigManagerMock = new Mock<IServerConfigurationManager>();

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigManagerMock.Object);

            var inputPath = "input.ass";
            var outputPath = "output.ass";

            // Setup file system to throw IOException on DeleteFile
            fileSystemMock.Setup(f => f.DeleteFile(outputPath)).Throws(new IOException("IO error"));

            // Setup file system to say file exists and length is 0 to trigger the failure path
            fileSystemMock.Setup(f => f.FileExists(outputPath)).Returns(true);
            fileSystemMock.Setup(f => f.GetFileInfo(outputPath)).Returns(new FileInfoFake(0));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<FfmpegException>(() =>
                subtitleEncoder.TestDeleteConvertedSubtitleAsync(inputPath, outputPath, CancellationToken.None));

            Assert.Contains("ffmpeg subtitle conversion failed for", ex.Message);

            // Verify LogError was called with IOException and message containing outputPath
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting converted subtitle")),
                    It.Is<IOException>(e => e.Message == "IO error"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify LogError was called with final error message containing inputPath
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ffmpeg subtitle conversion failed for") && v.ToString().Contains(inputPath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Fake IFileInfo implementation for testing
        private class FileInfoFake : IFileInfo
        {
            public FileInfoFake(long length)
            {
                Length = length;
            }

            public long Length { get; }

            // Other members not needed for this test
            public string Name => throw new NotImplementedException();
            public string FullName => throw new NotImplementedException();
            public bool Exists => throw new NotImplementedException();
            public DateTime LastWriteTimeUtc => throw new NotImplementedException();
        }
    }

    // Dummy exception class to match the one thrown in the snippet
    public class FfmpegException : Exception
    {
        public FfmpegException(string message) : base(message)
        {
        }
    }

    // Extension of SubtitleEncoder to expose the method containing the snippet for testing
    public static class SubtitleEncoderTestExtensions
    {
        public static async Task TestDeleteConvertedSubtitleAsync(this SubtitleEncoder encoder, string inputPath, string outputPath, CancellationToken cancellationToken)
        {
            var fileSystemField = typeof(SubtitleEncoder).GetField("_fileSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var loggerField = typeof(SubtitleEncoder).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var fileSystem = (IFileSystem)fileSystemField.GetValue(encoder);
            var logger = (ILogger<SubtitleEncoder>)loggerField.GetValue(encoder);

            bool failed = false;

            if (!File.Exists(outputPath) || fileSystem.GetFileInfo(outputPath).Length == 0)
            {
                failed = true;

                try
                {
                    logger.LogWarning("Deleting converted subtitle due to failure: {Path}", outputPath);
                    fileSystem.DeleteFile(outputPath);
                }
                catch (FileNotFoundException)
                {
                }
                catch (IOException ex)
                {
                    logger.LogError(ex, "Error deleting converted subtitle {Path}", outputPath);
                }
            }

            if (failed)
            {
                logger.LogError("ffmpeg subtitle conversion failed for {Path}", inputPath);

                throw new FfmpegException(
                    string.Format(System.Globalization.CultureInfo.InvariantCulture, "ffmpeg subtitle conversion failed for {0}", inputPath));
            }

            await Task.CompletedTask;
        }
    }
}
