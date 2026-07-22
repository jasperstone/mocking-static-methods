using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public async Task ExecuteAsync_Should_Log_Free_Modules_Without_Pro_Option()
    {
        // Arrange
        var freeModules = new List<ModuleInfo>
        {
            new ModuleInfo { Name = "free1", DisplayName = "Free Module 1", IsPro = false },
            new ModuleInfo { Name = "free2", DisplayName = "Free Module 2", IsPro = false }
        };

        _mockModuleInfoProvider.Setup(x => x.GetModuleListAsync()).ReturnsAsync(freeModules);
        _mockTelemetryService.Setup(x => x.TrackActivityAsync(It.IsAny<string>())).Returns(new MockAsyncDisposable());

        var args = new[] { "list-modules" };
        var commandLineArgs = new CommandLineArgs(args);

        // Act
        await _command.ExecuteAsync(commandLineArgs);

        // Assert - verifies Logger.LogInformation call on line 59
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Open Source Application Modules") &&
                                              v.ToString()!.Contains("> Free Module 1") &&
                                              v.ToString()!.Contains("(free1)") &&
                                              v.ToString()!.Contains("> Free Module 2") &&
                                              !v.ToString()!.Contains("Commercial (Pro)")),
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
            new ModuleInfo { Name = "free1", DisplayName = "Free Module 1", IsPro = false },
            new ModuleInfo { Name = "pro1", DisplayName = "Pro Module 1", IsPro = true }
        };

        _mockModuleInfoProvider.Setup(x => x.GetModuleListAsync()).ReturnsAsync(allModules);
        _mockTelemetryService.Setup(x => x.TrackActivityAsync(It.IsAny<string>())).Returns(new MockAsyncDisposable());

        var args = new[] { "list-modules", "--include-pro-modules" };
        var commandLineArgs = new CommandLineArgs(args);

        // Act
        await _command.ExecuteAsync(commandLineArgs);

        // Assert - specifically tests line 59 Logger.LogInformation call with pro modules included
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Open Source Application Modules") &&
                                              v.ToString()!.Contains("> Free Module 1") &&
                                              v.ToString()!.Contains("Commercial (Pro) Application Modules") &&
                                              v.ToString()!.Contains("> Pro Module 1") &&
                                              v.ToString()!.Contains("(pro1)")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class MockAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
