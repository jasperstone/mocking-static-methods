using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class NewCommandTests
{
    [Fact]
    public async Task Should_Log_Tiered_Yes_When_Tiered_Option_Is_Present()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var commandLineArgs = new CommandLineArgs
        {
            Target = "TestProject",
            Options = new Dictionary<string, string> { { "--tiered", "" } }
        };

        var command = CreateTestableCommand(loggerMock.Object);

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Tiered: yes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Not_Log_Tiered_Message_When_Tiered_Option_Is_Not_Present()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var commandLineArgs = new CommandLineArgs
        {
            Target = "TestProject",
            Options = new Dictionary<string, string>()
        };

        var command = CreateTestableCommand(loggerMock.Object);

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Tiered: yes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private static TestableNewCommand CreateTestableCommand(ILogger<NewCommand> logger)
    {
        var mockTemplateInfoProvider = new Mock<ITemplateInfoProvider>();
        mockTemplateInfoProvider.Setup(x => x.GetDefaultAsync()).ReturnsAsync(new TemplateInfo { Name = "app" });

        return new TestableNewCommand(
            new NullConnectionStringProvider(),
            new Mock<ISolutionPackageVersionFinder>().Object,
            new Mock<ICmdHelper>().Object,
            new Mock<IInstallLibsService>().Object,
            new Mock<CliService>().Object,
            new Mock<IAngularPwaSupportAdder>().Object,
            new Mock<IInitialMigrationCreator>().Object,
            new Mock<IThemePackageAdder>().Object,
            new Mock<ILocalEventBus>().Object,
            new Mock<IBundlingService>().Object,
            mockTemplateInfoProvider.Object,
            new Mock<TemplateProjectBuilder>().Object,
            new Mock<IAngularThemeConfigurer>().Object,
            new Mock<CliVersionService>().Object,
            new Mock<ITelemetryService>().Object,
            logger);
    }
}

public class NullConnectionStringProvider : IConnectionStringProvider
{
    public string GetConnectionString(string name) => null!;
}

public class TestableNewCommand : NewCommand
{
    public new ILogger<NewCommand> Logger { get; set; } = NullLogger<NewCommand>.Instance;

    public TestableNewCommand(
        IConnectionStringProvider connectionStringProvider,
        ISolutionPackageVersionFinder solutionPackageVersionFinder,
        ICmdHelper cmdHelper,
        IInstallLibsService installLibsService,
        CliService cliService,
        IAngularPwaSupportAdder angularPwaSupportAdder,
        IInitialMigrationCreator initialMigrationCreator,
        IThemePackageAdder themePackageAdder,
        ILocalEventBus eventBus,
        IBundlingService bundlingService,
        ITemplateInfoProvider templateInfoProvider,
        TemplateProjectBuilder templateProjectBuilder,
        IAngularThemeConfigurer angularThemeConfigurer,
        CliVersionService cliVersionService,
        ITelemetryService telemetryService,
        ILogger<NewCommand> logger)
        : base(connectionStringProvider, solutionPackageVersionFinder, cmdHelper, installLibsService, cliService,
               angularPwaSupportAdder, initialMigrationCreator, themePackageAdder, eventBus, bundlingService,
               angularThemeConfigurer, cliVersionService)
    {
        Logger = logger;
        TemplateInfoProvider = templateInfoProvider;
        TemplateProjectBuilder = templateProjectBuilder;
        _telemetryService = telemetryService;
    }

    // Override only what's necessary, don't try to override non-virtual methods
    protected Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
    {
        return Task.FromResult(new ProjectBuildArgs(
            new SolutionName(projectName),
            templateName: template,
            outputFolder: "test-output"
        ));
    }
}
