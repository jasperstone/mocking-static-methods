using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.ProjectBuilding;
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
    public async Task ExecuteAsync_Should_LogInformation_WithoutProModules()
    {
        // Arrange
        var freeModules = new List<object>
        {
            new { Name = "module1", DisplayName = "Module One", IsPro = false },
            new { Name = "module2", DisplayName = "Module Two", IsPro = false }
        };
        _mockModuleInfoProvider.Setup(x => x.GetModuleListAsync()).ReturnsAsync(freeModules.Cast<object>().ToList());

        var args = new CommandLineArgs();

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Open Source Application Modules") && 
                    v.ToString()!.Contains("> Module One".PadRight(50) + " (module1)") && 
                    v.ToString()!.Contains("> Module Two".PadRight(50) + " (module2)") &&
                    !v.ToString()!.Contains("Commercial (Pro) Application Modules")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogInformation_WithProModules()
    {
        // Arrange
        var allModules = new List<object>
        {
            new { Name = "free1", DisplayName = "Free Module", IsPro = false },
            new { Name = "pro1", DisplayName = "Pro Module", IsPro = true }
        };
        _mockModuleInfoProvider.Setup(x => x.GetModuleListAsync()).ReturnsAsync(allModules.Cast<object>().ToList());

        var options = new AbpCommandLineOptions();
        options["include-pro-modules"] = "";
        var args = new CommandLineArgs(null, null, options);

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Open Source Application Modules") &&
                    v.ToString()!.Contains("Commercial (Pro) Application Modules") &&
                    v.ToString()!.Contains("> Free Module".PadRight(50) + " (free1)") &&
                    v.ToString()!.Contains("> Pro Module".PadRight(50) + " (pro1)")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
