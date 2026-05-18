using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Jellyfin.Server.Helpers;
using MediaBrowser.Model.IO;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsAllInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPathsMock = new Mock<IApplicationPaths>();

            appPathsMock.Setup(x => x.ProgramDataPath).Returns("ProgramDataPath");
            appPathsMock.Setup(x => x.LogDirectoryPath).Returns("LogDirectoryPath");
            appPathsMock.Setup(x => x.ConfigurationDirectoryPath).Returns("ConfigurationDirectoryPath");
            appPathsMock.Setup(x => x.CachePath).Returns("CachePath");
            appPathsMock.Setup(x => x.TempDirectory).Returns("TempDirectory");
            appPathsMock.Setup(x => x.WebPath).Returns("WebPath");
            appPathsMock.Setup(x => x.ProgramSystemPath).Returns("ProgramSystemPath");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPathsMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(12));
        }
    }
}
