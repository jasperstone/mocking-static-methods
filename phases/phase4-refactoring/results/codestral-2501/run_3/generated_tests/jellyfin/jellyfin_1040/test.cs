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
using MediaBrowser.Controller.Configuration;
using System.Reflection;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        [Fact]
        public async Task Stop_LogsErrorWhenExceptionThrown()
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

            var processField = typeof(EncodedRecorder).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance);
            processField.SetValue(recorder, processMock.Object);

            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", BindingFlags.NonPublic | BindingFlags.Instance);
            targetPathField.SetValue(recorder, "testPath");

            // Act
            var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", BindingFlags.NonPublic | BindingFlags.Instance);
            stopMethod.Invoke(recorder, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
