using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Helpers.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_CallsLogInformationWithCorrectParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPathsMock = new Mock<IApplicationPaths>();

            // Setup mock properties
            appPathsMock.Setup(p => p.ProgramDataPath).Returns("/mock/program/data/path");
            appPathsMock.Setup(p => p.LogDirectoryPath).Returns("/mock/log/directory/path");
            appPathsMock.Setup(p => p.ConfigurationDirectoryPath).Returns("/mock/config/directory/path");
            appPathsMock.Setup(p => p.CachePath).Returns("/mock/cache/path");
            appPathsMock.Setup(p => p.TempDirectory).Returns("/mock/temp/directory");
            appPathsMock.Setup(p => p.WebPath).Returns("/mock/web/path");
            appPathsMock.Setup(p => p.ProgramSystemPath).Returns("/mock/program/system/path");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPathsMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s == "Web resources path: {WebPath}"),
                    It.Is<string>(s => s == "/mock/web/path")),
                Times.Once);

            // Additional verifications for other LogInformation calls can be added here
        }
    }
}
