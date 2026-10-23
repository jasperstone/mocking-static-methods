using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class ListModulesCommandTests
{
    private readonly Mock<ModuleInfoProvider> _mockModuleInfoProvider;
    private readonly Mock<ITelemetryService> _mockTelemetryService;
    private readonly Mock<ILogger<ListModulesCommand>> _mockLogger;
    private readonly ListModulesCommand _command;

    public ListModulesCommandTests()
    {
        _mockModuleInfoProvider = new Mock<ModuleInfoProvider>();
        _mockTelemetryService = new Mock<ITelemetryService>();
        _mockLogger = new Mock<ILogger<ListModulesCommand>>();

        _command = new ListModulesCommand(_mockModuleInfoProvider.Object, _mockTelemetryService.Object);
        _command.Logger = _mockLogger.Object;
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_Free_Modules_Without_Pro_Option()
    {
        // Arrange
        var freeModules = new List<ModuleInfo>
        {
            new() { Name = "free1", DisplayName = "Free Module 1", IsPro = false },
            new() { Name = "free2", DisplayName = "Free Module 2", IsPro = false }
        };

        _mockModuleInfoProvider.Setup(x => x.GetModuleListAsync()).Returns(Task.FromResult(freeModules));
        _mockTelemetryService.Setup(x => x.TrackActivityAsync(It.IsAny<string>())).Returns(Mock.Of<IAsyncDisposable>());

        var args = new CommandLineArgs();

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Free Module 1") && v.ToString()!.Contains("Free Module 2")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_Free_And_Pro_Modules_With_Pro_Option()
    {
        // Arrange
        var allModules = new List<ModuleInfo>
        {
            new() { Name = "free1", DisplayName = "Free Module 1", IsPro = false },
            new() { Name = "pro1", DisplayName = "Pro Module 1", IsPro = true }
        };

        _mockModuleInfoProvider.Setup(x => x.GetModuleListAsync()).Returns(Task.FromResult(allModules));
        _mockTelemetryService.Setup(x => x.TrackActivityAsync(It.IsAny<string>())).Returns(Mock.Of<IAsyncDisposable>());

        var options = new AbpCommandLineOptions();
        options.Add("include-pro-modules");
        var args = new CommandLineArgs(null, null);

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Open Source Application Modules") &&
                    v.ToString()!.Contains("Free Module 1") &&
                    v.ToString()!.Contains("Commercial (Pro) Application Modules") &&
                    v.ToString()!.Contains("Pro Module 1")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
