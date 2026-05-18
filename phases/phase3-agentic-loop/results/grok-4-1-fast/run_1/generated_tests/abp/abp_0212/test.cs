using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class NewCommandTests
{
    [Fact]
    public async Task Should_Log_Tiered_Yes_When_Tiered_Option_Is_Present()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NewCommand>>();
        var commandLineArgs = new CommandLineArgs(target: "MyProject")
        {
            Options = new AbpCommandLineOptions { { "--tiered", "" } }
        };

        var newCommand = new TestableNewCommand(mockLogger.Object);

        // Act
        await newCommand.ExecuteAsync(commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Tiered: yes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Not_Log_Tiered_When_Tiered_Option_Is_Not_Present()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NewCommand>>();
        var commandLineArgs = new CommandLineArgs(target: "MyProject")
        {
            Options = new AbpCommandLineOptions()
        };

        var newCommand = new TestableNewCommand(mockLogger.Object);

        // Act
        await newCommand.ExecuteAsync(commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Tiered: yes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}

public class TestableNewCommand : NewCommand
{
    public TestableNewCommand(ILogger<NewCommand> logger) : base(
        new TestConnectionStringProvider(),
        new TestSolutionPackageVersionFinder(),
        new TestCmdHelper(),
        new TestInstallLibsService(),
        new TestCliService(),
        new TestAngularPwaSupportAdder(),
        new TestInitialMigrationCreator(),
        new TestThemePackageAdder(),
        new TestLocalEventBus(),
        new TestBundlingService(),
        new TestTemplateInfoProvider(),
        new TestTemplateProjectBuilder(),
        new TestAngularThemeConfigurer(),
        new TestCliVersionService(),
        new TestTelemetryService())
    {
        Logger = logger;
    }

    protected override Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
    {
        return Task.FromResult(new ProjectBuildArgs
        {
            TemplateName = template,
            TargetFramework = "net8.0",
            OutputFolder = "test-output",
            ProjectName = projectName
        });
    }
}

// Minimal implementations for constructor dependencies
public class TestConnectionStringProvider : ConnectionStringProvider { }
public class TestSolutionPackageVersionFinder : SolutionPackageVersionFinder { }
public class TestCmdHelper : ICmdHelper { public Task RunAsync(string arguments, string workingDirectory = null, bool useShellExecute = false) => Task.CompletedTask; }
public class TestInstallLibsService : IInstallLibsService { public Task InstallLibsAsync(ProjectBuildArgs args) => Task.CompletedTask; }
public class TestCliService : CliService { }
public class TestAngularPwaSupportAdder { }
public class TestInitialMigrationCreator { public Task CreateAsync(ProjectBuildArgs args) => Task.CompletedTask; }
public class TestThemePackageAdder { }
public class TestLocalEventBus : ILocalEventBus 
{ 
    public IDisposable Register<T>(LocalEventHandler<T> handler) => null; 
    public IDisposable Register<T>(ILocalEventHandler<T> handler, bool onAssemblyLoaded = false) => null; 
    public Task PublishAsync<T>(T eventData) => Task.CompletedTask; 
    public Task PublishAsync<T>(T eventData, bool onUnitOfWorkComplete) => Task.CompletedTask; 
}
public class TestBundlingService : IBundlingService { public Task BundleAsync(ProjectBuildArgs args) => Task.CompletedTask; }
public class TestTemplateInfoProvider : ITemplateInfoProvider { public Task<TemplateInfo> GetDefaultAsync() => Task.FromResult(new TemplateInfo { Name = "app" }); }
public class TestTemplateProjectBuilder : TemplateProjectBuilder { public Task<byte[]> BuildAsync(ProjectBuildArgs args) => Task.FromResult(new byte[0]); }
public class TestAngularThemeConfigurer { }
public class TestCliVersionService : CliVersionService { }
public class TestTelemetryService : ITelemetryService { public Task AddActivityAsync(string activityName, Action<Dictionary<string, string>> propertiesAction) => Task.CompletedTask; }
