using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Helpers.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsWebPathInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.WebPath).Returns("/mock/web/path");

            // Act
            Jellyfin.Server.Helpers.StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - Verify the specific LogInformation call for WebPath (line 65 equivalent)
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t.ToString()!.Contains("Web resources path: {WebPath}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_CallsLogInformationWithWebPathParameter()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.WebPath).Returns("/path/to/web/resources");

            // Act
            Jellyfin.Server.Helpers.StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - Verify WebPath log call with parameter matching the mock value
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t.ToString()!.Contains("Web resources path: {WebPath}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>(),
                    It.Is<object[]>(args => args != null && args.Length == 1 && args[0]?.ToString() == "/path/to/web/resources")),
                Times.Once);
        }
    }
}
