using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

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
    public async Task ExecuteAsync_ShouldLogTieredYes_WhenTieredOptionPresent()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(null, "MyProject");
        commandLineArgs.Options["--tiered"] = null;

        _mockTemplateInfoProvider
            .Setup(x => x.GetDefaultAsync())
            .ReturnsAsync(new TemplateInfo { Name = "app" });

        _mockTemplateProjectBuilder
            .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(Array.Empty<byte>());

        var newCommand = CreateNewCommand();

        // Act
        await newCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v.ToString()).Contains("Tiered: yes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotLogTiered_WhenTieredOptionAbsent()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(null, "MyProject");

        _mockTemplateInfoProvider
            .Setup(x => x.GetDefaultAsync())
            .ReturnsAsync(new TemplateInfo { Name = "app" });

        _mockTemplateProjectBuilder
            .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(Array.Empty<byte>());

        var newCommand = CreateNewCommand();

        // Act
        await newCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v.ToString()).Contains("Tiered: yes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private NewCommand CreateNewCommand()
    {
        var mockConnectionStringProvider = new Mock<ConnectionStringProvider>();
        var mockSolutionPackageVersionFinder = new Mock<SolutionPackageVersionFinder>();
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockInstallLibsService = new Mock<IInstallLibsService>();
        var mockCliService = new Mock<CliService>();
        var mockAngularPwaSupportAdder = new Mock<AngularPwaSupportAdder>();
        var mockInitialMigrationCreator = new Mock<InitialMigrationCreator>();
        var mockThemePackageAdder = new Mock<ThemePackageAdder>();
        var mockEventBus = new Mock<ILocalEventBus>();
        var mockBundlingService = new Mock<IBundlingService>();
        var mockAngularThemeConfigurer = new Mock<AngularThemeConfigurer>();
        var mockCliVersionService = new Mock<CliVersionService>();
        var mockTelemetryService = new Mock<ITelemetryService>();

        mockTelemetryService
            .Setup(x => x.AddActivityAsync(It.IsAny<string>(), It.IsAny<Action<Dictionary<string, string>>>()))
            .Returns(Task.CompletedTask);

        return new NewCommand(
            mockConnectionStringProvider.Object,
            mockSolutionPackageVersionFinder.Object,
            mockCmdHelper.Object,
            mockInstallLibsService.Object,
            mockCliService.Object,
            mockAngularPwaSupportAdder.Object,
            mockInitialMigrationCreator.Object,
            mockThemePackageAdder.Object,
            mockEventBus.Object,
            mockBundlingService.Object,
            _mockTemplateInfoProvider.Object,
            _mockTemplateProjectBuilder.Object,
            mockAngularThemeConfigurer.Object,
            mockCliVersionService.Object,
            mockTelemetryService.Object)
        {
            Logger = _mockLogger.Object
        };
    }
}
