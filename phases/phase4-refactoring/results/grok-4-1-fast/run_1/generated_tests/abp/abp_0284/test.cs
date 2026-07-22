using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;
    private readonly SuiteCommand _suiteCommand;

    public SuiteCommandTests()
    {
        _mockCmdHelper = new Mock<ICmdHelper>();
        _mockLogger = new Mock<ILogger<SuiteCommand>>();

        _suiteCommand = new SuiteCommand(
            nuGetIndexUrlService: null!,
            packageVersionCheckerService: null!,
            cmdHelper: _mockCmdHelper.Object,
            authService: null!,
            cliHttpClientFactory: null!,
            suiteAppSettingsService: null!
        )
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public void StartSuite_Should_LogWarning_When_GlobalToolNotInstalled()
    {
        // Arrange - Mock static GlobalToolHelper.IsGlobalToolInstalled using file system
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            // Ensure tool is "not installed" by not creating the expected file
            // Act
            var result = InvokeStartSuite();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ABP Suite is not installed!")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once
            );

            Assert.Null(result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void StartSuite_Should_LogWarning_OnException_When_CheckingGlobalTool()
    {
        // Arrange - Create invalid path to cause File.Exists exception
        var originalHome = Environment.GetEnvironmentVariable("HOME");
        var originalUserProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        var fakeHome = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        Environment.SetEnvironmentVariable("HOME", fakeHome);
        Environment.SetEnvironmentVariable("USERPROFILE", fakeHome);
        
        // Make the path invalid by creating a directory with invalid chars or permissions
        try
        {
            var invalidPath = Path.Combine(fakeHome, ".dotnet", "tools");
            Directory.CreateDirectory(Path.Combine(fakeHome, ".dotnet"));
            
            // Create a file with invalid name to cause exception in path construction
            File.WriteAllText(Path.Combine(fakeHome, ".dotnet", "invalid{file"), "test");

            // Act
            var result = InvokeStartSuite();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Couldn't check ABP Suite installed status:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once
            );
        }
        finally
        {
            Environment.SetEnvironmentVariable("HOME", originalHome);
            Environment.SetEnvironmentVariable("USERPROFILE", originalUserProfile);
        }
    }

    private Process InvokeStartSuite()
    {
        return (Process)_suiteCommand.GetType()
            .GetMethod("StartSuite", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(_suiteCommand, null)!;
    }
}
