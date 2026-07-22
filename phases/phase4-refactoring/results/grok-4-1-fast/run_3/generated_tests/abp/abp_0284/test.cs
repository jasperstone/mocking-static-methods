using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly Mock<AbpNuGetIndexUrlService> _mockNuGetIndexUrlService;
    private readonly Mock<PackageVersionCheckerService> _mockPackageVersionCheckerService;
    private readonly Mock<AuthService> _mockAuthService;
    private readonly Mock<CliHttpClientFactory> _mockCliHttpClientFactory;
    private readonly Mock<SuiteAppSettingsService> _mockSuiteAppSettingsService;
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;

    public SuiteCommandTests()
    {
        _mockCmdHelper = new();
        _mockNuGetIndexUrlService = new();
        _mockPackageVersionCheckerService = new();
        _mockAuthService = new();
        _mockCliHttpClientFactory = new();
        _mockSuiteAppSettingsService = new();
        _mockLogger = new();
    }

    [Fact]
    public void StartSuite_Should_LogWarning_When_GlobalToolNotInstalled()
    {
        // Arrange - Use reflection to set private GlobalToolHelper field
        var suiteCommand = CreateSuiteCommand();
        var globalToolHelperField = typeof(SuiteCommand).GetField("GlobalToolHelper", 
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        globalToolHelperField.SetValue(null, _mockCmdHelper.Object);

        // Act
        var result = InvokeStartSuite(suiteCommand);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v.ToString()).Contains("ABP Suite is not installed!")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public void StartSuite_Should_LogWarning_OnException_When_CheckingGlobalTool()
    {
        // Arrange
        var suiteCommand = CreateSuiteCommand();
        var globalToolHelperField = typeof(SuiteCommand).GetField("GlobalToolHelper", 
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        globalToolHelperField.SetValue(null, _mockCmdHelper.Object);
        _mockCmdHelper.Setup(x => x.IsGlobalToolInstalled("abp-suite"))
            .Throws(new InvalidOperationException("Test exception"));

        // Act
        var result = InvokeStartSuite(suiteCommand);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v.ToString()).Contains("Couldn't check ABP Suite installed status:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private SuiteCommand CreateSuiteCommand()
    {
        var command = new SuiteCommand(
            _mockNuGetIndexUrlService.Object,
            _mockPackageVersionCheckerService.Object,
            _mockCmdHelper.Object,
            _mockAuthService.Object,
            _mockCliHttpClientFactory.Object,
            _mockSuiteAppSettingsService.Object)
        {
            Logger = _mockLogger.Object
        };
        return command;
    }

    private Process? InvokeStartSuite(SuiteCommand suiteCommand)
    {
        var method = typeof(SuiteCommand).GetMethod("StartSuite", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (Process?)method.Invoke(suiteCommand, null);
    }
}
