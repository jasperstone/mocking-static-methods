using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;
using Microsoft.Extensions.Configuration;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_ShouldLogEnvironmentVariablesAndSystemInfo()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            var envVars = new Dictionary<string, string>
            {
                { "JELLYFIN_TEST_VAR", "value1" },
                { "DOTNET_TEST_VAR", "value2" },
                { "OTHER_VAR", "value3" }
            };

            // Setup environment variables
            Environment.SetEnvironmentVariable("JELLYFIN_TEST_VAR", "value1");
            Environment.SetEnvironmentVariable("DOTNET_TEST_VAR", "value2");
            Environment.SetEnvironmentVariable("OTHER_VAR", "value3");

            // Setup ApplicationPaths mock
            mockAppPaths.SetupGet(p => p.ProgramDataPath).Returns("ProgramDataPath");
            mockAppPaths.SetupGet(p => p.LogDirectoryPath).Returns("LogDir");
            mockAppPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("ConfigDir");
            mockAppPaths.SetupGet(p => p.CachePath).Returns("CachePath");
            mockAppPaths.SetupGet(p => p.TempDirectory).Returns("TempDir");
            mockAppPaths.SetupGet(p => p.WebPath).Returns("WebPath");
            mockAppPaths.SetupGet(p => p.ProgramSystemPath).Returns("AppPath");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Environment Variables:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Arguments:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Check that system info logs are called
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Operating system:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
