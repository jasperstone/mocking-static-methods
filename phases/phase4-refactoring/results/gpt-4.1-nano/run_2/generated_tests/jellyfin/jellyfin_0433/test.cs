using System;
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
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.ProgramDataPath).Returns("/mock/program/data");
            mockAppPaths.SetupGet(p => p.LogDirectoryPath).Returns("/mock/log");
            mockAppPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("/mock/config");
            mockAppPaths.SetupGet(p => p.CachePath).Returns("/mock/cache");
            mockAppPaths.SetupGet(p => p.WebPath).Returns("/mock/web");
            mockAppPaths.SetupGet(p => p.ProgramSystemPath).Returns("/mock/app");

            // Set environment variables
            Environment.SetEnvironmentVariable("JELLYFIN_TEST_VAR", "value");
            Environment.SetEnvironmentVariable("JELLYFIN_ANOTHER_VAR", "another");
            Environment.SetEnvironmentVariable("DOTNET_TEST", "dotnet");
            Environment.SetEnvironmentVariable("OTHER_VAR", "notRelevant");

            // Set command line args
            Environment.SetCommandLineArgs(new string[] { "app.exe", "arg1", "arg2" });

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Environment Variables:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify that relevant env vars are included
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("JELLYFIN_TEST_VAR")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DOTNET_TEST")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Clean up environment variables
            Environment.SetEnvironmentVariable("JELLYFIN_TEST_VAR", null);
            Environment.SetEnvironmentVariable("JELLYFIN_ANOTHER_VAR", null);
            Environment.SetEnvironmentVariable("DOTNET_TEST", null);
            Environment.SetEnvironmentVariable("OTHER_VAR", null);
        }
    }
}
