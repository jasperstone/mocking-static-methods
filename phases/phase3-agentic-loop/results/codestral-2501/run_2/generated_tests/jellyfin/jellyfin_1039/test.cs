using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        [Fact]
        public async Task Stop_LogsInformation_WhenStoppingProcess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EncodedRecorder>>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

            var recorder = new EncodedRecorder(
                loggerMock.Object,
                mediaEncoderMock.Object,
                appPathsMock.Object,
                serverConfigurationManagerMock.Object);

            var mediaSource = new MediaSourceInfo();
            var targetFile = "test.ts";
            var duration = TimeSpan.FromSeconds(10);
            var cancellationToken = new CancellationToken();

            await recorder.Record(null, mediaSource, targetFile, duration, () => { }, cancellationToken);

            // Act
            recorder.Stop();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Stopping ffmpeg recording process for {Path}", It.IsAny<string>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation("Calling recording process.WaitForExit for {Path}", It.IsAny<string>()),
                Times.Once);
        }
    }
}
