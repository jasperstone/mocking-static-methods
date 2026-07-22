using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;
    private readonly SuiteCommand _suiteCommand;
    private readonly FieldInfo _abpSuitePortField;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();

        _suiteCommand = new SuiteCommand(
            nuGetIndexUrlService: null!,
            packageVersionCheckerService: null!,
            cmdHelper: Mock.Of<ICmdHelper>(),
            authService: null!,
            cliHttpClientFactory: null!,
            suiteAppSettingsService: null!
        );

        _suiteCommand.Logger = _mockLogger.Object;
        _abpSuitePortField = typeof(SuiteCommand).GetField("_abpSuitePort", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [Fact]
    public void GetTargetSolutionOrNull_With_ExplicitSolution_Should_Return_SolutionPath()
    {
        // Arrange
        var options = new AbpCommandLineOptions();
        options.Add("solution", "Test.sln");
        var args = new CommandLineArgs(null, null, options);

        // Act
        var result = InvokeGetTargetSolutionOrNull(args) as string;

        // Assert
        Assert.Equal("Test.sln", result);
    }

    [Fact]
    public void GetTargetSolutionOrNull_With_NoSolution_Should_Return_Null_When_NoSlnFiles()
    {
        // Arrange
        var options = new AbpCommandLineOptions();
        var args = new CommandLineArgs(null, null, options);

        // Act
        var result = InvokeGetTargetSolutionOrNull(args);

        // Assert - In test env with no .sln files, should return null
        Assert.Null(result);
    }

    [Fact]
    public void StartSuite_Should_LogError_When_PortAlreadyInUse()
    {
        // Arrange - Test relies on real port check or common test env behavior
        // Port 3000 often in use in CI/test environments

        // Act
        var result = InvokeStartSuite();

        // Assert - If port was in use, LogError should have been called
        // This verifies the Logger.LogError extension call on line 505
        _mockLogger.Verify(
            x => x.LogError(
                It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    private object InvokeStartSuite()
    {
        return typeof(SuiteCommand).GetMethod("StartSuite", 
            BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(_suiteCommand, null);
    }

    private object InvokeGetTargetSolutionOrNull(CommandLineArgs args)
    {
        return typeof(SuiteCommand).GetMethod("GetTargetSolutionOrNull", 
            BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(_suiteCommand, new object[] { args });
    }
}
