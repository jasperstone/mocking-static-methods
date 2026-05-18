using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
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
            var envVars = new Dictionary<string, object>
            {
                { "JELLYFIN_TEST", "value" },
                { "DOTNET_TEST", "value" },
                { "OTHER_VAR", "value" }
            };
            var envVarsCollection = envVars.ToDictionary(k => (object)k.Key, v => v.Value);
            var envVarsProvider = new Mock<IEnvironmentProvider>();
            envVarsProvider.Setup(e => e.GetEnvironmentVariables()).Returns(envVarsCollection);

            // Patch Environment static methods
            Environment.SetEnvironmentVariable("JELLYFIN_TEST", "value");
            Environment.SetEnvironmentVariable("DOTNET_TEST", "value");
            Environment.SetEnvironmentVariable("OTHER_VAR", "value");
            Environment.SetEnvironmentVariable("JELLYFIN_DATA_DIR", "/data");
            Environment.SetEnvironmentVariable("JELLYFIN_CONFIG_DIR", "/config");
            Environment.SetEnvironmentVariable("JELLYFIN_CACHE_DIR", "/cache");
            Environment.SetEnvironmentVariable("JELLYFIN_WEB_DIR", "/web");
            Environment.SetEnvironmentVariable("JELLYFIN_LOG_DIR", "/log");
            Environment.SetEnvironmentVariable("XDG_CACHE_HOME", "/xdg/cache");

            var appPaths = new Mock<IApplicationPaths>();
            appPaths.SetupGet(p => p.ProgramDataPath).Returns("/programdata");
            appPaths.SetupGet(p => p.LogDirectoryPath).Returns("/logdir");
            appPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("/conf");
            appPaths.SetupGet(p => p.CachePath).Returns("/cachepath");
            appPaths.SetupGet(p => p.TempDirectory).Returns("/temp");
            appPaths.SetupGet(p => p.WebPath).Returns("/web");
            appPaths.SetupGet(p => p.ProgramSystemPath).Returns("/appdir");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, appPaths.Object);

            // Assert
            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce());
        }
    }
}
