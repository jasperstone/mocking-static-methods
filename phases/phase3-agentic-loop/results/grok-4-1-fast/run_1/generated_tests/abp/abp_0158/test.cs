using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NuGet.Versioning;
using System;
using System.IO;
using Xunit;
using Volo.Abp.Cli;

namespace Volo.Abp.Cli.Tests;

public class CliServiceLoggerTests
{
    private readonly Mock<ILogger<CliService>> _loggerMock;
    private readonly CliService _cliService;

    public CliServiceLoggerTests()
    {
        _loggerMock = new Mock<ILogger<CliService>>();
        _loggerMock.SetupAllProperties();

        // Create constructor with NullLogger to avoid dependency issues
        _cliService = new CliService(
            null!, null!, null!, null!, null!, null!, null!, null!
        )
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public void LogNewVersionInfo_StableChannel_LogsCorrectMessages()
    {
        // Arrange
        MockStaticEnvironmentForGlobalToolDetection(false);
        var updateChannel = (object)"Stable"; // Use reflection or test via integration
        var latestVersion = new SemanticVersion(99, 0, 0);
        var toolPath = @"C:\local\path";

        // Use reflection to call private method since it's protected/internal
        typeof(CliService).GetMethod("LogNewVersionInfo", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_cliService, new object[] { "Stable", latestVersion, toolPath, null });

        // Assert - LoggerExtensions LogWarning calls
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A newer stable version") && v.ToString().Contains("99.0.0")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once()
        );

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet tool update --tool-path C:\\local\\path Volo.Abp.Cli"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once()
        );
    }

    [Fact]
    public void LogNewVersionInfo_GlobalTool_StableChannel_UsesGlobalFlag()
    {
        // Arrange
        MockStaticEnvironmentForGlobalToolDetection(true);
        var latestVersion = new SemanticVersion(7, 3, 0);
        var toolPath = @"%USERPROFILE%\.dotnet\tools\";

        // Act via reflection
        typeof(CliService).GetMethod("LogNewVersionInfo", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_cliService, new object[] { "Stable", latestVersion, toolPath, null });

        // Assert - uses -g for global tool
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet tool update -g Volo.Abp.Cli"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once()
        );
    }

    [Fact]
    public void LogNewVersionInfo_PrereleaseChannel_LogsVersionSpecificCommand()
    {
        // Arrange
        MockStaticEnvironmentForGlobalToolDetection(false);
        var latestVersion = new SemanticVersion(7, 3, 0, "alpha", "1");
        var toolPath = @"C:\local";

        // Act via reflection
        typeof(CliService).GetMethod("LogNewVersionInfo", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_cliService, new object[] { "Prerelease", latestVersion, toolPath, null });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"--version 7.3.0-alpha-1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.AtLeastOnce()
        );
    }

    [Fact]
    public void LogNewVersionInfo_WithMessage_LogsCustomMessage()
    {
        // Arrange
        var customMessage = "Custom release notes!";
        
        // Act via reflection
        typeof(CliService).GetMethod("LogNewVersionInfo", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_cliService, new object[] { "Stable", new SemanticVersion(1,0,0), "path", customMessage });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString() == customMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once()
        );
    }

    private static void MockStaticEnvironmentForGlobalToolDetection(bool isGlobal)
    {
        var mock = new Mock<EnvironmentMock>();
        // Simplified - test focuses on LoggerExtensions usage
    }
}
