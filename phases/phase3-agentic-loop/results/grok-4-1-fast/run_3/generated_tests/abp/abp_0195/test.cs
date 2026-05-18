using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
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

        _command = new ListModulesCommand(_mockModuleInfoProvider.Object, _mockTelemetryService.Object)
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_Free_Modules_Without_Pro_Option()
    {
        // Arrange
        var freeModules = new List<ModuleInfo>
        {
            new() { Name = "ModuleA", DisplayName = "Module A", IsPro = false },
            new() { Name = "ModuleB", DisplayName = "Module B", IsPro = false }
        };

        _mockModuleInfoProvider
            .Setup(x => x.GetModuleListAsync())
            .ReturnsAsync(freeModules);

        var disposable = new Mock<IDisposable>().Object;
        _mockTelemetryService
            .Setup(x => x.TrackActivityAsync(It.IsAny<string>()))
            .Returns(ValueTask.FromResult(disposable));

        var args = new CommandLineArgs(new string[] { });

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Open Source Application Modules") &&
                    v.ToString()!.Contains("> Module A") &&
                    v.ToString()!.Contains("> Module B")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_Free_And_Pro_Modules_With_Pro_Option()
    {
        // Arrange
        var freeModule = new ModuleInfo { Name = "ModuleA", DisplayName = "Module A", IsPro = false };
        var proModule = new ModuleInfo { Name = "ModulePro", DisplayName = "Module Pro", IsPro = true };
        var allModules = new List<ModuleInfo> { freeModule, proModule };

        _mockModuleInfoProvider
            .Setup(x => x.GetModuleListAsync())
            .ReturnsAsync(allModules);

        var disposable = new Mock<IDisposable>().Object;
        _mockTelemetryService
            .Setup(x => x.TrackActivityAsync(It.IsAny<string>()))
            .Returns(ValueTask.FromResult(disposable));

        var args = new CommandLineArgs(new[] { "--include-pro-modules" });

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Open Source Application Modules") &&
                    v.ToString()!.Contains("> Module A") &&
                    v.ToString()!.Contains("Commercial (Pro) Application Modules") &&
                    v.ToString()!.Contains("> Module Pro")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Empty_Free_Modules()
    {
        // Arrange
        _mockModuleInfoProvider
            .Setup(x => x.GetModuleListAsync())
            .ReturnsAsync(new List<ModuleInfo>());

        var disposable = new Mock<IDisposable>().Object;
        _mockTelemetryService
            .Setup(x => x.TrackActivityAsync(It.IsAny<string>()))
            .Returns(ValueTask.FromResult(disposable));

        var args = new CommandLineArgs(new string[] { });

        // Act
        await _command.ExecuteAsync(args);

        // Assert - Should log header even with no modules
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Open Source Application Modules")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
