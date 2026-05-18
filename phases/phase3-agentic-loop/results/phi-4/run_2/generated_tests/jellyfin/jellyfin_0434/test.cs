using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.ServerSetupApp; // Ensure this using directive is included

namespace Jellyfin.Server.Helpers.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_CallsLogInformationWithExpectedParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.Setup(p => p.ProgramSystemPath).Returns("/mock/application/path");

            var expectedApplicationPath = "/mock/application/path";

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Application directory: {ApplicationPath}")),
                    It.Is<object>(o => o.ToString() == expectedApplicationPath)),
                Times.Once);
        }
    }
}
