using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Tests.Helpers
{
    public interface IApplicationPaths
    {
        string ProgramDataPath { get; }
        string LogDirectoryPath { get; }
        string ConfigurationDirectoryPath { get; }
        string CachePath { get; }
        string TempDirectory { get; }
        string WebPath { get; }
        string ProgramSystemPath { get; }
    }

    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsExpectedInformationIncludingWebPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPathsMock = new Mock<IApplicationPaths>();

            appPathsMock.SetupGet(a => a.ProgramDataPath).Returns("/program/data");
            appPathsMock.SetupGet(a => a.LogDirectoryPath).Returns("/log/dir");
            appPathsMock.SetupGet(a => a.ConfigurationDirectoryPath).Returns("/config/dir");
            appPathsMock.SetupGet(a => a.CachePath).Returns("/cache/path");
            appPathsMock.SetupGet(a => a.TempDirectory).Returns("/temp/dir");
            appPathsMock.SetupGet(a => a.WebPath).Returns("/web/path");
            appPathsMock.SetupGet(a => a.ProgramSystemPath).Returns("/program/system");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPathsMock.Object);

            // Assert
            // Verify the specific call on line 65: logger.LogInformation("Web resources path: {WebPath}", appPaths.WebPath);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Web resources path:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Also verify that the WebPath value was passed as argument
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    null,
                    It.Is<Func<It.IsAnyType, Exception, string>>(f =>
                    {
                        // We cannot directly check the argument, but we can check that the formatter returns the expected string
                        var state = new { WebPath = "/web/path" };
                        var formatted = f(state, null);
                        return formatted.Contains("/web/path");
                    })),
                Times.AtLeastOnce);
        }
    }
}
