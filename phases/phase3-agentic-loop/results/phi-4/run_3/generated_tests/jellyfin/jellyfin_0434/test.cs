using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Runtime.InteropServices;

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
            mockAppPaths.Setup(p => p.ProgramDataPath).Returns("/mock/program/data/path");
            mockAppPaths.Setup(p => p.LogDirectoryPath).Returns("/mock/log/directory/path");
            mockAppPaths.Setup(p => p.ConfigurationDirectoryPath).Returns("/mock/config/directory/path");
            mockAppPaths.Setup(p => p.CachePath).Returns("/mock/cache/path");
            mockAppPaths.Setup(p => p.TempDirectory).Returns("/mock/temp/directory");
            mockAppPaths.Setup(p => p.WebPath).Returns("/mock/web/path");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Application directory: {ApplicationPath}")),
                    It.Is<object>(o => o.ToString() == "/mock/application/path")),
                Times.Once);
        }
    }

    public interface IApplicationPaths
    {
        string ProgramSystemPath { get; }
        string ProgramDataPath { get; }
        string LogDirectoryPath { get; }
        string ConfigurationDirectoryPath { get; }
        string CachePath { get; }
        string TempDirectory { get; }
        string WebPath { get; }
    }

    public class StartupHelpers
    {
        private static readonly string[] _relevantEnvVarPrefixes = { "JELLYFIN_", "DOTNET_", "ASPNETCORE_" };

        public static void LogEnvironmentInfo(ILogger logger, IApplicationPaths appPaths)
        {
            var commandLineArgs = Environment.GetCommandLineArgs().Distinct();

            var allEnvVars = Environment.GetEnvironmentVariables();
            var relevantEnvVars = new Dictionary<object, object>();
            foreach (var key in allEnvVars.Keys)
            {
                if (_relevantEnvVarPrefixes.Any(prefix => key.ToString()!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    relevantEnvVars.Add(key, allEnvVars[key]!);
                }
            }

            logger.LogInformation("Environment Variables: {EnvVars}", relevantEnvVars);
            logger.LogInformation("Arguments: {Args}", commandLineArgs);
            logger.LogInformation("Operating system: {OS}", RuntimeInformation.OSDescription);
            logger.LogInformation("Architecture: {Architecture}", RuntimeInformation.OSArchitecture);
            logger.LogInformation("64-Bit Process: {Is64Bit}", Environment.Is64BitProcess);
            logger.LogInformation("User Interactive: {IsUserInteractive}", Environment.UserInteractive);
            logger.LogInformation("Processor count: {ProcessorCount}", Environment.ProcessorCount);
            logger.LogInformation("Program data path: {ProgramDataPath}", appPaths.ProgramDataPath);
            logger.LogInformation("Log directory path: {LogDirectoryPath}", appPaths.LogDirectoryPath);
            logger.LogInformation("Config directory path: {ConfigurationDirectoryPath}", appPaths.ConfigurationDirectoryPath);
            logger.LogInformation("Cache path: {CachePath}", appPaths.CachePath);
            logger.LogInformation("Temp directory path: {TempDirPath}", appPaths.TempDirectory);
            logger.LogInformation("Web resources path: {WebPath}", appPaths.WebPath);
            logger.LogInformation("Application directory: {ApplicationPath}", appPaths.ProgramSystemPath);
        }
    }
}
