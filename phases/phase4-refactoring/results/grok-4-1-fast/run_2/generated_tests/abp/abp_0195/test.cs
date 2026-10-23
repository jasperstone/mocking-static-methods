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

        _command = new ListModulesCommand(
            _mockModuleInfoProvider.Object,
            _mockTelemetryService.Object
        );
        _command.Logger = _mockLogger.Object;
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_Free_Modules_Without_Pro_Option()
    {
        // Arrange
        var freeModules = new List<ModuleInfo>
        {
            new() { Name = "module1", DisplayName = "Module One", IsPro = false },
            new() { Name = "module2", DisplayName = "Module Two", IsPro = false }
        };

        _mockModuleInfoProvider
            .Setup(x => x.GetModuleListAsync())
            .ReturnsAsync(freeModules);

        _mockTelemetryService
            .Setup(x => x.TrackActivityAsync(It.IsAny<string>()))
            .Returns(ValueTask.FromResult(Mock.Of<IAsyncDisposable>()));

        var args = new CommandLineArgs(Array.Empty<string>());

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v!.ToString()!.Contains("Open Source Application Modules") && 
                    v.ToString()!.Contains("> Module One") &&
                    v.ToString()!.Contains("(module1)") &&
                    v.ToString()!.Contains("> Module Two") &&
                    v.ToString()!.Contains("(module2)") &&
                    !v.ToString()!.Contains("Commercial (Pro)")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_Free_And_Pro_Modules_With_Pro_Option()
    {
        // Arrange
        var freeModules = new List<ModuleInfo>
        {
            new() { Name = "free1", DisplayName = "Free Module", IsPro = false }
        };
        var proModules = new List<ModuleInfo>
        {
            new() { Name = "pro1", DisplayName = "Pro Module", IsPro = true }
        };
        var allModules = freeModules.Concat(proModules).ToList();

        _mockModuleInfoProvider
            .Setup(x => x.GetModuleListAsync())
            .ReturnsAsync(allModules);

        _mockTelemetryService
            .Setup(x => x.TrackActivityAsync(It.IsAny<string>()))
            .Returns(ValueTask.FromResult(Mock.Of<IAsyncDisposable>()));

        var args = new CommandLineArgs(new[] { "--include-pro-modules" });

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v!.ToString()!.Contains("Open Source Application Modules") &&
                    v.ToString()!.Contains("> Free Module") &&
                    v.ToString()!.Contains("(free1)") &&
                    v.ToString()!.Contains("Commercial (Pro) Application Modules") &&
                    v.ToString()!.Contains("> Pro Module") &&
                    v.ToString()!.Contains("(pro1)")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_Should_Log_Only_Free_Modules_When_No_Pro_Modules_Exist()
    {
        // Arrange
        var modules = new List<ModuleInfo>
        {
            new() { Name = "module1", DisplayName = "Module One", IsPro = false }
        };

        _mockModuleInfoProvider
            .Setup(x => x.GetModuleListAsync())
            .ReturnsAsync(modules);

        _mockTelemetryService
            .Setup(x => x.TrackActivityAsync(It.IsAny<string>()))
            .Returns(ValueTask.FromResult(Mock.Of<IAsyncDisposable>()));

        var args = new CommandLineArgs(new[] { "--include-pro-modules" });

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v!.ToString()!.Contains("Open Source Application Modules") &&
                    v.ToString()!.Contains("> Module One") &&
                    !v.ToString()!.Contains("Commercial (Pro)")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }
}
