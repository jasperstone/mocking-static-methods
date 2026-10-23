using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class NewCommandTests
{
    private readonly Mock<ILogger<NewCommand>> _mockLogger;

    public NewCommandTests()
    {
        _mockLogger = new Mock<ILogger<NewCommand>>();
    }

    [Fact]
    public async Task Should_Log_Tiered_Yes_When_Tiered_Option_Is_Present()
    {
        // Arrange
        var options = new AbpCommandLineOptions();
        options.Add("--tiered", "");
        
        var commandLineArgs = new CommandLineArgs(null, "MyProject");
        typeof(CommandLineArgs).GetField("<Options>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(commandLineArgs, options);

        var mockTemplateInfoProvider = new Mock<ITemplateInfoProvider>();
        mockTemplateInfoProvider.Setup(x => x.GetDefaultAsync())
            .ReturnsAsync(new Mock<TemplateInfo>("app").Object);

        var newCommand = CreateNewCommand(mockTemplateInfoProvider.Object);

        // Act
        await newCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state?.ToString()?.Contains("Tiered: yes") ?? false),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Not_Log_Tiered_Message_When_Tiered_Option_Is_Not_Present()
    {
        // Arrange
        var options = new AbpCommandLineOptions();
        var commandLineArgs = new CommandLineArgs(null, "MyProject");
        typeof(CommandLineArgs).GetField("<Options>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(commandLineArgs, options);

        var mockTemplateInfoProvider = new Mock<ITemplateInfoProvider>();
        mockTemplateInfoProvider.Setup(x => x.GetDefaultAsync())
            .ReturnsAsync(new Mock<TemplateInfo>("app").Object);

        var newCommand = CreateNewCommand(mockTemplateInfoProvider.Object);

        // Act
        await newCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state?.ToString()?.Contains("Tiered: yes") ?? false),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private NewCommand CreateNewCommand(ITemplateInfoProvider templateInfoProvider)
    {
        var mockTemplateProjectBuilder = new Mock<TemplateProjectBuilder>();
        mockTemplateProjectBuilder.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new byte[0]);

        var mockTelemetryService = new Mock<ITelemetryService>();
        mockTelemetryService.Setup(x => x.AddActivityAsync(It.IsAny<string>(), It.IsAny<Action<Dictionary<string, string>>>()))
            .Returns(Task.CompletedTask);

        // Create mocks for concrete types using interfaces where possible
        var mocks = new Dictionary<Type, object>
        {
            [typeof(object)] = new object(), // fallback for concrete types we can't resolve
        };

        return new NewCommand(
            new object(), // ConnectionStringProvider
            new object(), // SolutionPackageVersionFinder  
            Mock.Of<ICmdHelper>(),
            Mock.Of<IInstallLibsService>(),
            new object(), // CliService
            new object(), // AngularPwaSupportAdder
            new object(), // InitialMigrationCreator
            new object(), // ThemePackageAdder
            Mock.Of<ILocalEventBus>(),
            Mock.Of<IBundlingService>(),
            templateInfoProvider,
            mockTemplateProjectBuilder.Object,
            new object(), // AngularThemeConfigurer
            new object(), // CliVersionService
            mockTelemetryService.Object)
        {
            Logger = _mockLogger.Object
        };
    }
}
