using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Jellyfin.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_ShouldLogEnvironmentVariablesAndSystemInfo()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.ProgramDataPath).Returns("/mock/path/data");
            mockAppPaths.SetupGet(p => p.LogDirectoryPath).Returns("/mock/path/log");
            mockAppPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("/mock/path/config");
            mockAppPaths.SetupGet(p => p.CachePath).Returns("/mock/path/cache");
            mockAppPaths.SetupGet(p => p.WebPath).Returns("/mock/path/web");
            mockAppPaths.SetupGet(p => p.ProgramSystemPath).Returns("/mock/path/system");

            // Save original environment variables
            var originalEnvVars = Environment.GetEnvironmentVariables();

            // Set environment variables for test
            Environment.SetEnvironmentVariable("JELLYFIN_TEST_VAR", "value");
            Environment.SetEnvironmentVariable("DOTNET_TEST_VAR2", "value2");
            Environment.SetEnvironmentVariable("UNRELATED_VAR", "should not be included");

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
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Arguments:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Operating system:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Architecture:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup environment variables
            foreach (DictionaryEntry envVar in originalEnvVars)
            {
                Environment.SetEnvironmentVariable((string)envVar.Key, (string)envVar.Value);
            }
        }
    }
}
