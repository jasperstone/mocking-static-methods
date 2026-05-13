using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.MediaEncoding
{
    public class TranscodingSegmentCleanerTests
    {
        [Fact]
        public async Task DeleteSegmentFiles_LogsExpectedDebugMessage()
        {
            // Arrange
            const long minIndex = 1;
            const long maxIndex = 3;
            var segmentPath = Path.Combine(Path.GetTempPath(), "output.m3u8");

            var job = new TranscodingJob
            {
                Path = segmentPath,
                Type = TranscodingJobType.Hls
            };

            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var encoderMock = new Mock<IMediaEncoder>();

            fileSystemMock.Setup(fs => fs.GetFilePaths(It.IsAny<string>()))
                .Returns(Array.Empty<string>());

            var cleaner = new TranscodingSegmentCleaner(
                job,
                loggerMock.Object,
                configMock.Object,
                fileSystemMock.Object,
                encoderMock.Object,
                segmentLength: 10);

            var method = typeof(TranscodingSegmentCleaner).GetMethod(
                "DeleteSegmentFiles",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);

            // Act
            var task = (Task)method.Invoke(cleaner, new object[] { job, minIndex, maxIndex, 0 })!;
            await task.ConfigureAwait(false);

            // Assert
            var expectedMessage = $"Deleting segment file(s) index {minIndex} to {maxIndex} from {segmentPath}";
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                    {
                        if (state is null)
                        {
                            return false;
                        }

                        if (!string.Equals(state.ToString(), expectedMessage, StringComparison.Ordinal))
                        {
                            return false;
                        }

                        if (state is IReadOnlyList<KeyValuePair<string, object>> stateList)
                        {
                            return stateList.Any(kv => kv.Key == "Min" && Equals(kv.Value, minIndex))
                                && stateList.Any(kv => kv.Key == "Max" && Equals(kv.Value, maxIndex))
                                && stateList.Any(kv => kv.Key == "Path" && Equals(kv.Value, segmentPath));
                        }

                        return false;
                    }),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
