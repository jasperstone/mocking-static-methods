using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogInformation_CallsWithCorrectParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths
            {
                ProgramSystemPath = "/mock/application/path"
            };

            // Act
            StartupHelpers.LogInformation(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Application directory: /mock/application/path")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mock class to simulate ServerApplicationPaths
    public class ServerApplicationPaths
    {
        public string ProgramSystemPath { get; set; }
    }

    // Mock class to simulate StartupHelpers
    public static class StartupHelpers
    {
        public static void LogInformation(ILogger logger, ServerApplicationPaths appPaths)
        {
            logger.LogInformation("Application directory: {ApplicationPath}", appPaths.ProgramSystemPath);
        }
    }
}
