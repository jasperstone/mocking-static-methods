using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Model.IO;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger<EncodedRecorder>> _mockLogger;
        private readonly Mock<IMediaEncoder> _mockMediaEncoder;
        private readonly Mock<IServerApplicationPaths> _mockAppPaths;
        private readonly Mock<IServerConfigurationManager> _mockServerConfigurationManager;
        private readonly EncodedRecorder _encodedRecorder;

        public EncodedRecorderTests()
        {
            _mockLogger = new Mock<ILogger<EncodedRecorder>>();
            _mockMediaEncoder = new Mock<IMediaEncoder>();
            _mockAppPaths = new Mock<IServerApplicationPaths>();
            _mockServerConfigurationManager = new Mock<IServerConfigurationManager>();

            _encodedRecorder = new EncodedRecorder(
                _mockLogger.Object,
                _mockMediaEncoder.Object,
                _mockAppPaths.Object,
                _mockServerConfigurationManager.Object);
        }

        [Fact]
        public async Task Record_ShouldLogError_WhenStoppingRecordingProcessFails()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "test.ts";
            var duration = TimeSpan.FromSeconds(10);
            var cancellationToken = new CancellationToken();
            var process = new Process();
            var processStartInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardError = true
            };
            process.StartInfo = processStartInfo;
            process.Start();

            _mockMediaEncoder.Setup(m => m.EncoderPath).Returns("ffmpeg");
            _mockAppPaths.Setup(a => a.LogDirectoryPath).Returns("logs");

            // Act
            await _encodedRecorder.Record(null, mediaSource, targetFile, duration, () => { }, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error stopping recording transcoding job for test.ts")),
                    It.IsAny<Exception>()),
                Times.Once);
        }

        [Fact]
        public async Task Record_ShouldLogError_WhenWaitingForRecordingProcessToExitFails()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "test.ts";
            var duration = TimeSpan.FromSeconds(10);
            var cancellationToken = new CancellationToken();
            var process = new Process();
            var processStartInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardError = true
            };
            process.StartInfo = processStartInfo;
            process.Start();

            _mockMediaEncoder.Setup(m => m.EncoderPath).Returns("ffmpeg");
            _mockAppPaths.Setup(a => a.LogDirectoryPath).Returns("logs");

            // Act
            await _encodedRecorder.Record(null, mediaSource, targetFile, duration, () => { }, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error waiting for recording process to exit for test.ts")),
                    It.IsAny<Exception>()),
                Times.Once);
        }

        [Fact]
        public async Task Record_ShouldLogError_WhenKillingRecordingProcessFails()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "test.ts";
            var duration = TimeSpan.FromSeconds(10);
            var cancellationToken = new CancellationToken();
            var process = new Process();
            var processStartInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardError = true
            };
            process.StartInfo = processStartInfo;
            process.Start();

            _mockMediaEncoder.Setup(m => m.EncoderPath).Returns("ffmpeg");
            _mockAppPaths.Setup(a => a.LogDirectoryPath).Returns("logs");

            // Act
            await _encodedRecorder.Record(null, mediaSource, targetFile, duration, () => { }, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error killing recording transcoding job for test.ts")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }
}
