using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliService_LogNewVersionInfo_Tests
    {
        [Theory]
        [InlineData("Stable", "-g")]
        [InlineData("Prerelease", "--tool-path C:\\tools")]
        public void LogNewVersionInfo_LogsExpectedWarnings(string updateChannelName, string expectedToolPathArg)
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = CreateCliServiceWithLogger(loggerMock.Object);

            // Use reflection to get the private method LogNewVersionInfo
            var method = typeof(CliService).GetMethod("LogNewVersionInfo", BindingFlags.NonPublic | BindingFlags.Instance);

            var latestVersion = new SemanticVersion(1, 2, 3);
            var toolPath = expectedToolPathArg == "-g" ? Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.dotnet\tools\") : "C:\\tools";
            string message = "Custom message";

            // Parse the enum value from string
            var updateChannelEnum = Enum.Parse(Type.GetType("Volo.Abp.Cli.CliService+UpdateChannel, Volo.Abp.Cli.Core"), updateChannelName);

            // Act
            method.Invoke(cliService, new object[] { updateChannelEnum, latestVersion, toolPath, message });

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"A newer {updateChannelName.ToLowerInvariant()} version of the ABP CLI is available: {latestVersion}.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == string.Empty),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(2));

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Update Command: "),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            if (updateChannelName == "Stable")
            {
                loggerMock.Verify(l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"dotnet tool update {expectedToolPathArg} Volo.Abp.Cli"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            }
            else if (updateChannelName == "Prerelease")
            {
                loggerMock.Verify(l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"dotnet tool update {expectedToolPathArg} Volo.Abp.Cli --version {latestVersion}"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            }
        }

        private CliService CreateCliServiceWithLogger(ILogger<CliService> logger)
        {
            var commandLineArgumentParser = new Mock<Volo.Abp.Cli.Args.ICommandLineArgumentParser>().Object;
            var commandSelector = new Mock<Volo.Abp.Cli.Commands.ICommandSelector>().Object;
            var serviceScopeFactory = new Mock<IServiceScopeFactory>().Object;
            var packageVersionCheckerService = new Mock<Volo.Abp.Cli.Version.PackageVersionCheckerService>().Object;
            var cmdHelper = new Mock<Volo.Abp.Cli.ICmdHelper>().Object;
            var memoryService = new Mock<Volo.Abp.Cli.Memory.MemoryService>().Object;
            var cliVersionService = new Mock<Volo.Abp.Cli.CliVersionService>().Object;
            var telemetryService = new Mock<Volo.Abp.Internal.Telemetry.ITelemetryService>().Object;

            var cliService = new CliService(
                commandLineArgumentParser,
                commandSelector,
                serviceScopeFactory,
                packageVersionCheckerService,
                cmdHelper,
                memoryService,
                cliVersionService,
                telemetryService);

            cliService.Logger = logger;
            return cliService;
        }
    }
}
