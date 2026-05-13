using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.LiveTv.Tests
{
    public class EncodedRecorderTests
    {
        [Fact]
        public async Task Record_CallsLogInformationOnStop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var directStreamProviderMock = new Mock<IDirectStreamProvider>();
            var mediaSource = new MediaSourceInfo { Path = "path" };
            var targetFile = "targetFile";
            var duration = TimeSpan.FromMinutes(1);
            var onStarted = new Action(() => { });
            var cancellationTokenSource = new CancellationTokenSource();

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(directStreamProviderMock.Object, mediaSource, targetFile, duration, onStarted, cancellationTokenSource.Token);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Record_CallsLogInformationOnWaitForExit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var directStreamProviderMock = new Mock<IDirectStreamProvider>();
            var mediaSource = new MediaSourceInfo { Path = "path" };
            var targetFile = "targetFile";
            var duration = TimeSpan.FromMinutes(1);
            var onStarted = new Action(() => { });
            var cancellationTokenSource = new CancellationTokenSource();

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(directStreamProviderMock.Object, mediaSource, targetFile, duration, onStarted, cancellationTokenSource.Token);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Record_CallsLogInformationOnKill()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var directStreamProviderMock = new Mock<IDirectStreamProvider>();
            var mediaSource = new MediaSourceInfo { Path = "path" };
            var targetFile = "targetFile";
            var duration = TimeSpan.FromMinutes(1);
            var onStarted = new Action(() => { });
            var cancellationTokenSource = new CancellationTokenSource();

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(directStreamProviderMock.Object, mediaSource, targetFile, duration, onStarted, cancellationTokenSource.Token);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
