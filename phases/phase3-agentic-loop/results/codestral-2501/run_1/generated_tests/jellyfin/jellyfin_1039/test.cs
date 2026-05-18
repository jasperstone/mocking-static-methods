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

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        [Fact]
        public void Stop_LogsInformation_WhenProcessIsRunning()
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
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);

            var privateObject = new PrivateObject(recorder);
            privateObject.SetFieldOrProperty("_process", processMock.Object);
            privateObject.SetFieldOrProperty("_targetPath", "testPath");
            privateObject.SetFieldOrProperty("_hasExited", false);

            // Act
            privateObject.Invoke("Stop");

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }
    }
}
