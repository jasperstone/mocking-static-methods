using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class EncodedRecorderTests
    {
        [Fact]
        public async Task Record_CompletesSuccessfully()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var directStreamProviderMock = new Mock<IDirectStreamProvider>();
            var mediaSourceInfo = new MediaSourceInfo { Path = "test.mp4" };
            var targetFile = "output.ts";
            var duration = TimeSpan.FromMinutes(1);

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(directStreamProviderMock.Object, mediaSourceInfo, targetFile, duration, () => { }, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Stop_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var directStreamProviderMock = new Mock<IDirectStreamProvider>();
            var mediaSourceInfo = new MediaSourceInfo { Path = "test.mp4" };
            var targetFile = "output.ts";
            var duration = TimeSpan.FromMinutes(1);

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);

            // Act
            encodedRecorder.Stop();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
