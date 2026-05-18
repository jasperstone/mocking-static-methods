using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
    }

    [Fact]
    public async Task ExecuteAsync_RemoveOperation_ShouldLogRemovingABP_Suite()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockNuGetService = new Mock<AbpNuGetIndexUrlService>();
        var mockAuthService = new Mock<AuthService>();
        mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(new object()); // Avoid auth check
        
        var suiteCommand = new SuiteCommand(
            mockNuGetService.Object,
            new object(), // PackageVersionCheckerService
            mockCmdHelper.Object,
            mockAuthService.Object,
            new object(), // CliHttpClientFactory
            new object()  // SuiteAppSettingsService
        );
        suiteCommand.Logger = _mockLogger.Object;

        var args = new CommandLineArgs(command: "suite", target: "remove");

        // Act
        await suiteCommand.ExecuteAsync(args);

        // Assert - Verifies Logger.LogInformation("Removing ABP Suite...") extension call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Removing ABP Suite")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once()
        );
    }

    [Fact]
    public async Task GenerateCrudPageAsync_ShouldLogGeneratingCRUDPage()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockNuGetService = new Mock<AbpNuGetIndexUrlService>();
        var suiteCommand = new SuiteCommand(
            mockNuGetService.Object,
            new object(),
            mockCmdHelper.Object,
            new object(),
            new object(),
            new object()
        );
        suiteCommand.Logger = _mockLogger.Object;

        var args = new CommandLineArgs(command: "suite", target: "generate");

        // Act & Assert - Log happens before validation throws UserFriendlyException
        await Assert.ThrowsAsync<UserFriendlyException>(() => suiteCommand.GenerateCrudPageAsync(args));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating CRUD Page")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once()
        );
    }

    [Fact]
    public void Constructor_ShouldSetLoggerProperty()
    {
        // Arrange & Act
        var suiteCommand = new SuiteCommand(
            new Mock<AbpNuGetIndexUrlService>().Object,
            new object(),
            new Mock<ICmdHelper>().Object,
            new object(),
            new object(),
            new object()
        );

        // Assert - Tests that Logger property is properly initialized for LogInformation extension usage
        Assert.NotNull(suiteCommand.Logger);
    }
}
