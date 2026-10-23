using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_ShouldLogEnvironmentVariablesAndSystemInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new DummyApplicationPaths();

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            // Verify that LogInformation was called at least once with LogLevel.Information
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }

        private class DummyApplicationPaths : IApplicationPaths
        {
            public string ProgramDataPath => "dummyProgramDataPath";
            public string LogDirectoryPath => "dummyLogDirectoryPath";
            public string ConfigurationDirectoryPath => "dummyConfigDirectoryPath";
            public string CachePath => "dummyCachePath";
            public string TempDirectory => "dummyTempDirectory";
            public string WebPath => "dummyWebPath";
            public string ProgramSystemPath => "dummyProgramSystemPath";
        }
    }
}
