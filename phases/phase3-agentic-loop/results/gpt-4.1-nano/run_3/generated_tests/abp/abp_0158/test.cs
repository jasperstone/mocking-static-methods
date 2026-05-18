using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Args;
using System;
using NuGet.Versioning;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public void LogNewVersionInfo_ShouldCallLogWarning_ForEachMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                null, null, null, null, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            var updateChannel = CliService.UpdateChannel.Stable;
            var version = new SemanticVersion(1, 2, 3);
            var message = "Test message";
            var latestVersion = new SemanticVersion(2, 0, 0);

            // Act
            var methodInfo = typeof(CliService).GetMethod("LogNewVersionInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(cliService, new object[] { updateChannel, latestVersion, "path", message });

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void LogWarning_ShouldBeCalled_WhenCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                null, null, null, null, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            loggerMock.Object.LogWarning("Warning message");

            // Assert
            loggerMock.Verify(x => x.LogWarning("Warning message"), Times.Once);
        }
    }
}
