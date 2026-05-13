using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Extensions;
using Jellyfin.Extensions.Json;
using Jellyfin.LiveTv.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        [Fact]
        public async Task RecordFromFile_LogsErrorWhenWaitingForProcessExitFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

            var recorder = new EncodedRecorder(
                loggerMock.Object,
                mediaEncoderMock.Object,
                appPathsMock.Object,
                serverConfigurationManagerMock.Object);

            var mediaSource = new MediaSourceInfo { Path = "testPath" };
            var targetFile = "testTargetFile.ts";
            var cancellationToken = CancellationToken.None;

            // Simulate process that does not exit
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            processMock.Setup(p => p.Kill()).Verifiable();

            recorder._process = processMock.Object;

            // Act
            await recorder.RecordFromFile(mediaSource, mediaSource.Path, targetFile, () => { }, cancellationToken);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Error waiting for recording process to exit for {Path}",
                    It.Is<string>(path => path == targetFile)),
                Times.Once);
        }
    }
}
