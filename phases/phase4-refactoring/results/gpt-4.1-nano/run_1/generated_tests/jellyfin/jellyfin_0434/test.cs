using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_ShouldLogExpectedInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new Mock<IApplicationPaths>();
            appPaths.SetupGet(p => p.ProgramDataPath).Returns("/mock/program/data");
            appPaths.SetupGet(p => p.LogDirectoryPath).Returns("/mock/log");
            appPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("/mock/config");
            appPaths.SetupGet(p => p.CachePath).Returns("/mock/cache");
            appPaths.SetupGet(p => p.WebPath).Returns("/mock/web");
            appPaths.SetupGet(p => p.ProgramSystemPath).Returns("/mock/app");

            // Setup environment variables
            var envVars = new Dictionary<string, string>
            {
                { "JELLYFIN_TEST_VAR", "value" },
                { "DOTNET_TEST_VAR", "value" },
                { "OTHER_VAR", "value" }
            };
            Environment.SetEnvironmentVariable("JELLYFIN_TEST_VAR", "value");
            Environment.SetEnvironmentVariable("DOTNET_TEST_VAR", "value");
            Environment.SetEnvironmentVariable("OTHER_VAR", "value");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths.Object);

            // Assert
            // Verify that LogInformation was called at least once
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Environment Variables:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
