using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Jellyfin.LiveTv.IO;

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
            var cancellationToken = new CancellationToken();

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(directStreamProviderMock.Object, mediaSource, targetFile, duration, () => { }, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Stop_CallsLogInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var processMock = new Mock<Process>();
            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object)
            {
                _process = processMock.Object,
                _targetPath = "targetPath"
            };

            // Act
            encodedRecorder.Stop();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
