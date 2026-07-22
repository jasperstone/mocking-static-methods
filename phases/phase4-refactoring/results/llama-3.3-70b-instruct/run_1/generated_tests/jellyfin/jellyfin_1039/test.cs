using Xunit;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Dto;

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
            var appPathsMock = new Mock<MediaBrowser.Common.Configuration.IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var directStreamProviderMock = new Mock<MediaBrowser.Model.IO.IDirectStreamProvider>();
            var mediaSourceInfo = new MediaSourceInfo { Path = "test.mp4" };
            var targetFile = "test.ts";
            var duration = TimeSpan.FromMinutes(1);

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(directStreamProviderMock.Object, mediaSourceInfo, targetFile, duration, () => { }, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Stop_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<MediaBrowser.Common.Configuration.IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var directStreamProviderMock = new Mock<MediaBrowser.Model.IO.IDirectStreamProvider>();
            var mediaSourceInfo = new MediaSourceInfo { Path = "test.mp4" };
            var targetFile = "test.ts";
            var duration = TimeSpan.FromMinutes(1);

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(directStreamProviderMock.Object, mediaSourceInfo, targetFile, duration, () => { }, CancellationToken.None);
            encodedRecorder.Stop();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }
}
