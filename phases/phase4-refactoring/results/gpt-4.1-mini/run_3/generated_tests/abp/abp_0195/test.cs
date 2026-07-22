using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationWithFreeModulesOnly_WhenNoProModulesOption()
        {
            // Arrange
            var moduleList = new List<Volo.Abp.Cli.ProjectBuilding.ModuleInfo>
            {
                new Volo.Abp.Cli.ProjectBuilding.ModuleInfo("Module1", "Module 1 Display", false),
                new Volo.Abp.Cli.ProjectBuilding.ModuleInfo("Module2", "Module 2 Display", false),
                new Volo.Abp.Cli.ProjectBuilding.ModuleInfo("ProModule", "Pro Module Display", true)
            };

            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>(
                MockBehavior.Strict,
                null, null, null, null);
            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).Returns(Task.FromResult(moduleList));

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(t => t.TrackActivityAsync(It.IsAny<string>())).Returns(Task.FromResult<IAsyncDisposable>(new DummyAsyncDisposable()));

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Open Source Application Modules") && v.ToString().Contains("Module 1 Display") && !v.ToString().Contains("Pro Module Display")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsInformationWithProModules_WhenIncludeProModulesOption()
        {
            // Arrange
            var moduleList = new List<Volo.Abp.Cli.ProjectBuilding.ModuleInfo>
            {
                new Volo.Abp.Cli.ProjectBuilding.ModuleInfo("Module1", "Module 1 Display", false),
                new Volo.Abp.Cli.ProjectBuilding.ModuleInfo("ProModule", "Pro Module Display", true)
            };

            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>(
                MockBehavior.Strict,
                null, null, null, null);
            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).Returns(Task.FromResult(moduleList));

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(t => t.TrackActivityAsync(It.IsAny<string>())).Returns(Task.FromResult<IAsyncDisposable>(new DummyAsyncDisposable()));

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs();
            args.Options["include-pro-modules"] = "";

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Commercial (Pro) Application Modules") && v.ToString().Contains("Pro Module Display")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        private class DummyAsyncDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);
        }
    }
}
