using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class ListModulesCommandTests
{
    private class ModuleInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsPro { get; set; }
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogInformation_Without_ProModules_When_No_IncludeProOption()
    {
        // Arrange
        var modules = new List<ModuleInfo>
        {
            new() { Name = "free1", DisplayName = "Free Module 1", IsPro = false },
            new() { Name = "free2", DisplayName = "Free Module 2", IsPro = false }
        };

        var moduleProviderMock = new Mock<ModuleInfoProvider>();
        moduleProviderMock.Setup(m => m.GetModuleListAsync()).Returns(Task.FromResult(modules));

        var telemetryMock = new Mock<ITelemetryService>();
        var asyncDisposableMock = new Mock<IAsyncDisposable>();
        telemetryMock.Setup(x => x.TrackActivityAsync(It.IsAny<string>())).Returns(Task.FromResult(asyncDisposableMock.Object));

        var loggerMock = new Mock<ILogger<ListModulesCommand>>();

        var command = new ListModulesCommand(moduleProviderMock.Object, telemetryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(args);

        // Assert - verifies LogInformation was called (line 59)
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => ((string)state.ToString()).Contains("Open Source Application Modules")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogInformation_With_ProModules_When_IncludeProOption()
    {
        // Arrange
        var modules = new List<ModuleInfo>
        {
            new() { Name = "free1", DisplayName = "Free Module 1", IsPro = false },
            new() { Name = "pro1", DisplayName = "Pro Module 1", IsPro = true }
        };

        var moduleProviderMock = new Mock<ModuleInfoProvider>();
        moduleProviderMock.Setup(m => m.GetModuleListAsync()).Returns(Task.FromResult(modules));

        var telemetryMock = new Mock<ITelemetryService>();
        var asyncDisposableMock = new Mock<IAsyncDisposable>();
        telemetryMock.Setup(x => x.TrackActivityAsync(It.IsAny<string>())).Returns(Task.FromResult(asyncDisposableMock.Object));

        var loggerMock = new Mock<ILogger<ListModulesCommand>>();

        var command = new ListModulesCommand(moduleProviderMock.Object, telemetryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs();
        args.Options["include-pro-modules"] = "";

        // Act
        await command.ExecuteAsync(args);

        // Assert - verifies LogInformation was called with pro modules (line 59)
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => ((string)state.ToString()).Contains("Commercial (Pro) Application Modules")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
