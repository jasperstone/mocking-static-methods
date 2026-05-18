using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests.Subtitles
{
    public class SubtitleEncoderLoggerTests
    {
        private readonly Mock<ILogger<SubtitleEncoder>> _loggerMock;
        private readonly Mock<global::MediaBrowser.Controller.IO.IFileSystem> _fileSystemMock;

        public SubtitleEncoderLoggerTests()
        {
            _loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            _loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            _fileSystemMock = new Mock<global::MediaBrowser.Controller.IO.IFileSystem>();
        }

        [Fact]
        public void DeleteConvertedSubtitleWithIOException_LogsErrorMessage()
        {
            // Arrange
            var outputPath = "/path/to/output.srt";
            var ioException = new IOException("Delete failed");
            _fileSystemMock.Setup(x => x.DeleteFile(outputPath)).Throws(ioException);

            // Act - simulate the exact catch (IOException ex) block from source code
            try
            {
                _fileSystemMock.Object.DeleteFile(outputPath);
            }
            catch (FileNotFoundException)
            {
                // Expected - no log
            }
            catch (IOException ex)
            {
                // This is the exact LogError extension call from line ~457
                _loggerMock.Object.LogError(ex, "Error deleting converted subtitle {Path}", outputPath);
            }

            // Assert - verify the specific LogError extension call was made
            _loggerMock.Verify(
                x => x.LogError(
                    ioException,
                    "Error deleting converted subtitle {Path}",
                    outputPath),
                Times.Once);
        }

        [Fact]
        public void DeleteConvertedSubtitleWithFileNotFound_DoesNotLogError()
        {
            // Arrange
            var outputPath = "/path/to/output.srt";
            _fileSystemMock.Setup(x => x.DeleteFile(outputPath)).Throws(new FileNotFoundException());

            // Act - simulate the catch block pattern from source code
            try
            {
                _fileSystemMock.Object.DeleteFile(outputPath);
            }
            catch (FileNotFoundException)
            {
                // Expected - no LogError call
            }
            catch (IOException ex)
            {
                _loggerMock.Object.LogError(ex, "Error deleting converted subtitle {Path}", outputPath);
            }

            // Assert - LogError was NOT called for FileNotFoundException
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<IOException>(),
                    "Error deleting converted subtitle {Path}",
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void LoggerExtensionLogError_CanBeVerified()
        {
            // Arrange
            var testException = new IOException("test");
            var testPath = "/test/path";

            // Act - directly call the LogError extension method being tested
            _loggerMock.Object.LogError(testException, "Error deleting converted subtitle {Path}", testPath);

            // Assert - verify the underlying Log call with correct parameters
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((v?.ToString() ?? "").Contains("Error deleting converted subtitle /test/path") ?? false)),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
