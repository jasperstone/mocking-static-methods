using System;
using System.IO;
using System.Threading;
using Jellyfin.LiveTv.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller;
using System.Diagnostics;

namespace Jellyfin.LiveTv.Tests
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigurationManagerMock;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public void RecordFromFile_LogsError_WhenProcessFailsToExit()
        {
            // Arrange
            var mediaSource = new MediaBrowser.MediaInfo.MediaSourceInfo { Path = "test.mp4" };
            var targetFile = "test.ts";
            var inputFile = "test.mp4";
            var onStarted = new Action(() => { });
            var cancellationToken = new CancellationToken();

            var processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,

                RedirectStandardError = true,
                RedirectStandardInput = true,

                FileName = "ffmpeg",
                Arguments = "-i test.mp4 -c:v copy test.ts",

                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = false
            };

            _mediaEncoderMock.Setup(m => m.EncoderPath).Returns("ffmpeg");
            _appPathsMock.Setup(a => a.LogDirectoryPath).Returns(Directory.GetCurrentDirectory());
            _serverConfigurationManagerMock.Setup(s => s.GetConfiguration).Returns(new ServerConfiguration());

            var encodedRecorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigurationManagerMock.Object);

            // Act
            encodedRecorder.Record(new Mock<IDirectStreamProvider>().Object, mediaSource, targetFile, TimeSpan.FromSeconds(10), onStarted, cancellationToken);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error waiting for recording process to exit for {Path}", targetFile), Times.Once);
        }
    }
}
