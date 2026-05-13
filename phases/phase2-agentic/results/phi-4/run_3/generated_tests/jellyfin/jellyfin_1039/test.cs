using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        [Fact]
        public void Stop_ShouldLogInformationWhenCallingWaitForExit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EncodedRecorder>>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

            var recorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object)
            {
                _targetPath = "testPath",
                _process = new Process
                {
                    HasExited = false
                }
            };

            // Act
            recorder.Stop();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Calling recording process.WaitForExit for {Path}", It.Is<string>(s => s == "testPath")),
                Times.Once);
        }
    }
}
