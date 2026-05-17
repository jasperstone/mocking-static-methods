using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
    private readonly Mock<AbpNuGetIndexUrlService> _nuGetIndexUrlServiceMock;
    private readonly Mock<object> _packageVersionCheckerServiceMock; // Use object since type not found
    private readonly Mock<object> _cmdHelperMock; // Use object since ICmdHelper not found
    private readonly Mock<object> _authServiceMock; // Use object since type not found
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
    private readonly SuiteCommand _suiteCommand;

    public SuiteCommandTests()
    {
        _loggerMock = new Mock<ILogger<SuiteCommand>>();
        _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        _packageVersionCheckerServiceMock = new Mock<object>();
        _cmdHelperMock = new Mock<object>();
        _authServiceMock = new Mock<object>();
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        _suiteCommand = new SuiteCommand(
            _nuGetIndexUrlServiceMock.Object,
            _packageVersionCheckerServiceMock.Object,
            _cmdHelperMock.Object,
            _authServiceMock.Object,
            _cliHttpClientFactoryMock.Object,
            _suiteAppSettingsServiceMock.Object
        );
        _suiteCommand.Logger = _loggerMock.Object;
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogPreviewVersionInformation()
    {
        // Arrange
        _nuGetIndexUrlServiceMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync("https://example.com/nuget");

        // Act
        await _suiteCommand.InstallSuiteAsync(null, preview: true);

        // Assert - Verifies the LogInformation extension call on line ~300 is executed
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(1));
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogInfoTextBeforeNugetCheck()
    {
        // Arrange
        _nuGetIndexUrlServiceMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync((string)null);

        // Act
        await _suiteCommand.InstallSuiteAsync(null, preview: false);

        // Assert - Verifies the first LogInformation call (lines 270-300 range)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(1));
    }

    [Fact]
    public async Task ExecuteAsync_RemoveOperation_ShouldLogRemovingMessage()
    {
        // Arrange
        var args = new CommandLineArgs { Target = "remove" };

        // Act
        await _suiteCommand.ExecuteAsync(args);

        // Assert - Verifies Logger.LogInformation("Removing ABP Suite...")
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Removing ABP Suite") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_SetsLoggerUsingExtensionPattern()
    {
        // Arrange & Act
        var command = new SuiteCommand(
            Mock.Of<AbpNuGetIndexUrlService>(),
            new object(), // PackageVersionCheckerService
            new object(), // ICmdHelper
            new object(), // AuthService
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        );

        // Assert - Tests that Logger property is set (uses Microsoft.Extensions.Logging.LoggerExtensions pattern)
        Assert.NotNull(command.Logger);
    }
}
