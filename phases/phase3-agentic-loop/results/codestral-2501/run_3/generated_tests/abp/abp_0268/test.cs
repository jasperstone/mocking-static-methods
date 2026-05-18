using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Licensing;
using Volo.Abp.Cli.Utils;
using Xunit;

public class SuiteCommandTests
{
    [Fact]
    public async Task InstallSuiteAsync_ShouldLogInformation_WhenNuGetIndexUrlIsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        nuGetIndexUrlServiceMock.Setup(service => service.GetAsync()).ReturnsAsync((string)null);

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new CommandLineArgs("install", new AbpCommandLineOptions());

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogInformation_WhenPreviewIsTrue()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        nuGetIndexUrlServiceMock.Setup(service => service.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new CommandLineArgs("install", new AbpCommandLineOptions
        {
            { "preview", "true" }
        });

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(It.Is<string>(s => s.Contains("latest preview version"))),
            Times.Once
        );
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogInformation_WhenVersionIsSpecified()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        nuGetIndexUrlServiceMock.Setup(service => service.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new CommandLineArgs("install", new AbpCommandLineOptions
        {
            { "version", "1.0.0" }
        });

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(It.Is<string>(s => s.Contains("v1.0.0"))),
            Times.Once
        );
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogInformation_WhenLatestPreviewVersionIsNotNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(Mock.Of<IApiKeyService>());
        nuGetIndexUrlServiceMock.Setup(service => service.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new CommandLineArgs("install", new AbpCommandLineOptions
        {
            { "preview", "true" }
        });

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(It.Is<string>(s => s.Contains("Latest preview version is"))),
            Times.Once
        );
    }
}
