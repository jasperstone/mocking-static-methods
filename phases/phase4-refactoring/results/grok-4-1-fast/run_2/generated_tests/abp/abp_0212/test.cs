using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class NewCommandTests
{
    private readonly Mock<ILogger<NewCommand>> _mockLogger;
    private readonly Mock<ITemplateInfoProvider> _mockTemplateInfoProvider;
    private readonly Mock<TemplateProjectBuilder> _mockTemplateProjectBuilder;

    public NewCommandTests()
    {
        _mockLogger = new Mock<ILogger<NewCommand>>();
        _mockTemplateInfoProvider = new Mock<ITemplateInfoProvider>();
        _mockTemplateProjectBuilder = new Mock<TemplateProjectBuilder>();
    }

    [Fact]
    public async Task Should_Log_Tiered_Yes_When_Tiered_Option_Is_Present()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(target: "TestProject");
        commandLineArgs.Options["--tiered"] = "";

        _mockTemplateInfoProvider.Setup(x => x.GetDefaultAsync())
            .ReturnsAsync(new TemplateInfo("app") { Name = "app" });

        _mockTemplateProjectBuilder.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new byte[0]);

        var newCommand = CreateNewCommand();

        // Act
        await newCommand.ExecuteAsync(commandLineArgs);

        // Assert - Verify the specific LogInformation call for tiered
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Tiered: yes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Not_Log_Tiered_Message_When_Tiered_Option_Is_Not_Present()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(target: "TestProject");

        _mockTemplateInfoProvider.Setup(x => x.GetDefaultAsync())
            .ReturnsAsync(new TemplateInfo("app") { Name = "app" });

        _mockTemplateProjectBuilder.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new byte[0]);

        var newCommand = CreateNewCommand();

        // Act
        await newCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Tiered: yes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private NewCommand CreateNewCommand()
    {
        var mocks = CreateAllMocks();

        return new NewCommand(
            mocks[typeof(ConnectionStringProvider)].Object as ConnectionStringProvider!,
            mocks[typeof(SolutionPackageVersionFinder)].Object as SolutionPackageVersionFinder!,
            mocks[typeof(ICmdHelper)].Object as ICmdHelper!,
            mocks[typeof(IInstallLibsService)].Object as IInstallLibsService!,
            mocks[typeof(CliService)].Object as CliService!,
            mocks[typeof(AngularPwaSupportAdder)].Object as AngularPwaSupportAdder!,
            mocks[typeof(InitialMigrationCreator)].Object as InitialMigrationCreator!,
            mocks[typeof(ThemePackageAdder)].Object as ThemePackageAdder!,
            mocks[typeof(ILocalEventBus)].Object as ILocalEventBus!,
            mocks[typeof(IBundlingService)].Object as IBundlingService!,
            _mockTemplateInfoProvider.Object,
            _mockTemplateProjectBuilder.Object,
            mocks[typeof(AngularThemeConfigurer)].Object as AngularThemeConfigurer!,
            mocks[typeof(CliVersionService)].Object as CliVersionService!,
            new Mock<object>().Object) // telemetryService
        {
            Logger = _mockLogger.Object
        };
    }

    private Dictionary<Type, Mock> CreateAllMocks()
    {
        return new Dictionary<Type, Mock>
        {
            [typeof(ConnectionStringProvider)] = new Mock<ConnectionStringProvider>(),
            [typeof(SolutionPackageVersionFinder)] = new Mock<SolutionPackageVersionFinder>(),
            [typeof(ICmdHelper)] = new Mock<ICmdHelper>(),
            [typeof(IInstallLibsService)] = new Mock<IInstallLibsService>(),
            [typeof(CliService)] = new Mock<CliService>(),
            [typeof(AngularPwaSupportAdder)] = new Mock<AngularPwaSupportAdder>(),
            [typeof(InitialMigrationCreator)] = new Mock<InitialMigrationCreator>(),
            [typeof(ThemePackageAdder)] = new Mock<ThemePackageAdder>(),
            [typeof(ILocalEventBus)] = new Mock<ILocalEventBus>(),
            [typeof(IBundlingService)] = new Mock<IBundlingService>(),
            [typeof(AngularThemeConfigurer)] = new Mock<AngularThemeConfigurer>(),
            [typeof(CliVersionService)] = new Mock<CliVersionService>()
        };
    }
}

public class TemplateInfo
{
    public string Name { get; set; } = "";
    public TemplateInfo(string name) => Name = name;
}
