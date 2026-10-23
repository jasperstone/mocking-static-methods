using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli;
using System;
using System.Reflection;
using NuGet.Versioning;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public void LogNewVersionInfo_Should_Log_Warning_Messages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                null, // CommandLineArgumentParser
                null, // CommandSelector
                null, // ServiceScopeFactory
                null, // PackageVersionCheckerService
                null, // CmdHelper
                null, // MemoryService
                null, // CliVersionService
                null  // ITelemetryService
            )
            {
                Logger = loggerMock.Object
            };

            // Use reflection to get the internal enum type
            var updateChannelType = typeof(CliService).GetNestedType("UpdateChannel", BindingFlags.NonPublic);
            // Create an enum value for 'Stable'
            var updateChannelValue = Enum.ToObject(updateChannelType, 1); // assuming 'Stable' is 1

            var latestVersion = new SemanticVersion(1, 2, 3);
            var toolPath = "/usr/local/bin/abp";

            // Invoke the private method via reflection
            var methodInfo = typeof(CliService).GetMethod("LogNewVersionInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(cliService, new object[] { updateChannelValue, latestVersion, toolPath, "Test message" });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A newer") && v.ToString().Contains("version of the ABP CLI is available")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
