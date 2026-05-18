using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;
    private readonly Mock<IOptionsSnapshot<Volo.Abp.Cli.AbpCliOptions>> _mockCliOptions;

    public SuiteCommandTests()
    {
        _mockCmdHelper = new Mock<ICmdHelper>();
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
        _mockCliOptions = new Mock<IOptionsSnapshot<Volo.Abp.Cli.AbpCliOptions>>();
        _mockCliOptions.Setup(x => x.Value).Returns(new Volo.Abp.Cli.AbpCliOptions());
    }

    [Fact]
    public void StartSuite_Should_LogWarning_When_GlobalTool_NotInstalled()
    {
        // Arrange
        var suiteCommand = CreateSuiteCommand();
        
        // Mock GlobalToolHelper static call using It.IsAny
        Mock.Get(suiteCommand.Logger)
            .Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString() == "ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        var result = suiteCommand.StartSuite();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString() == "ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.Null(result);
    }

    [Fact]
    public void StartSuite_Should_LogWarning_OnException_When_Checking_GlobalTool()
    {
        // Arrange
        var suiteCommand = CreateSuiteCommand();
        
        // Act
        var result = suiteCommand.StartSuite();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Couldn't check ABP Suite installed status:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private SuiteCommand CreateSuiteCommand()
    {
        // Create minimal mocks for dependencies that don't affect StartSuite
        var mockNuGetService = new Mock<Volo.Abp.Cli.Commands.Services.AbpNuGetIndexUrlService>().Object;
        var mockVersionService = new Mock<Volo.Abp.Cli.Commands.Services.PackageVersionCheckerService>().Object;
        var mockAuthService = new Mock<Volo.Abp.Cli.Auth.AuthService>().Object;
        var mockHttpFactory = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>().Object;
        var mockSettingsService = new Mock<Volo.Abp.Cli.Commands.Services.SuiteAppSettingsService>().Object;
        
        var cmdHelper = new CmdHelper(_mockCliOptions.Object);

        var suiteCommand = new SuiteCommand(
            mockNuGetService,
            mockVersionService,
            cmdHelper,
            mockAuthService,
            mockHttpFactory,
            mockSettingsService
        );

        suiteCommand.Logger = _mockLogger.Object;
        return suiteCommand;
    }
}
