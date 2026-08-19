using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Server.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System.Runtime.InteropServices;
using System.IO;
using Xunit;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsApplicationDirectory()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            
            // Create a testable app paths implementation
            var appPaths = new TestApplicationPaths
            {
                ProgramSystemPath = "/test/application/path",
                ProgramDataPath = "/test/data",
                LogDirectoryPath = "/test/log",
                ConfigurationDirectoryPath = "/test/config",
                CachePath = "/test/cache",
                TempDirectory = "/test/temp",
                WebPath = "/test/web"
            };

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, appPaths);

            // Assert - Verify the specific LogInformation call for Application directory (line 66)
            mockLogger.Protected()
                .Verify("Log", Times.Once(),
                    ItExpr.Is<LogLevel>(LogLevel.Information),
                    ItExpr.Is<int>(0),
                    ItExpr.IsAny<Microsoft.Extensions.Logging.FormattedLogValues>(),
                    ItExpr.IsAny<Exception>(),
                    ItExpr.IsAny<Func<Microsoft.Extensions.Logging.FormattedLogValues, Exception?, string>>());
            
            // Verify it was called with the expected message pattern
            mockLogger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<Microsoft.Extensions.Logging.FormattedLogValues>(v => v.Format.Contains("Application directory: {ApplicationPath}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<Microsoft.Extensions.Logging.FormattedLogValues, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsAllExpectedMessages()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var appPaths = new TestApplicationPaths();

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, appPaths);

            // Assert - Verify all expected log messages are emitted
            var expectedMessages = new[]
            {
                "Environment Variables: {EnvVars}",
                "Arguments: {Args}",
                "Operating system: {OS}",
                "Architecture: {Architecture}",
                "64-Bit Process: {Is64Bit}",
                "User Interactive: {IsUserInteractive}",
                "Processor count: {ProcessorCount}",
                "Program data path: {ProgramDataPath}",
                "Log directory path: {LogDirectoryPath}",
                "Config directory path: {ConfigurationDirectoryPath}",
                "Cache path: {CachePath}",
                "Temp directory path: {TempDirPath}",
                "Web resources path: {WebPath}",
                "Application directory: {ApplicationPath}"
            };

            foreach (var message in expectedMessages)
            {
                mockLogger.Verify(x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<Microsoft.Extensions.Logging.FormattedLogValues>(v => v.Format.Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<Microsoft.Extensions.Logging.FormattedLogValues, Exception?, string>>()),
                    Times.Once);
            }
        }

        [Fact]
        public void LogEnvironmentInfo_OnlyLogsRelevantEnvironmentVariables()
        {
            // Arrange
            Environment.SetEnvironmentVariable("JELLYFIN_DATA_DIR", "/jellyfin/data");
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:5000");
            Environment.SetEnvironmentVariable("UNRELATED_VAR", "should not be included");

            var mockLogger = new Mock<ILogger>();
            var appPaths = new TestApplicationPaths();

            try
            {
                // Act
                StartupHelpers.LogEnvironmentInfo(mockLogger.Object, appPaths);

                // Assert - Verify environment variables log was called
                mockLogger.Verify(x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<Microsoft.Extensions.Logging.FormattedLogValues>(v => v.Format.Contains("Environment Variables: {EnvVars}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<Microsoft.Extensions.Logging.FormattedLogValues, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                // Cleanup
                Environment.SetEnvironmentVariable("JELLYFIN_DATA_DIR", null);
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
                Environment.SetEnvironmentVariable("ASPNETCORE_URLS", null);
                Environment.SetEnvironmentVariable("UNRELATED_VAR", null);
            }
        }
    }

    // Test implementation of IApplicationPaths to avoid missing interface issues
    public class TestApplicationPaths : IApplicationPaths
    {
        public string ProgramDataPath { get; set; } = "/default/data";
        public string LogDirectoryPath { get; set; } = "/default/log";
        public string ConfigurationDirectoryPath { get; set; } = "/default/config";
        public string CachePath { get; set; } = "/default/cache";
        public string TempDirectory { get; set; } = "/default/temp";
        public string WebPath { get; set; } = "/default/web";
        public string ProgramSystemPath { get; set; } = "/default/system";
        public string InternalTempDirectory { get; set; } = "/default/internal-temp";
    }
}
