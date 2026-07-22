using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        [Fact]
        public async Task Stop_LogsError_WhenExceptionThrown()
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

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(new InvalidOperationException("Test exception"));

            var targetPath = "testPath";
            var processField = typeof(EncodedRecorder).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processField.SetValue(recorder, processMock.Object);

            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            targetPathField.SetValue(recorder, targetPath);

            // Act
            recorder.Stop();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
