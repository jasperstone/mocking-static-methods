using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsFormattedOutput_WithAndWithoutProModules()
        {
            // Arrange
            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>(null!, null!, null!, null!);
            moduleInfoProviderMock
                .Setup(m => m.GetModuleListAsync())
                .ReturnsAsync(new[]
                {
                    CreateModuleInfo("FreeModule", "Free Display", isPro: false),
                    CreateModuleInfo("ProModule", "Pro Display", isPro: true)
                });

            var telemetryMock = new Mock<ITelemetryService>();
            telemetryMock
                .Setup(t => t.TrackActivityAsync(ActivityNameConsts.AbpCliCommandsListModules))
                .Returns(new FakeAsyncDisposable());

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var argsWithoutPro = new CommandLineArgs(Array.Empty<string>());
            var argsWithPro = new CommandLineArgs(new[] { "--include-pro-modules" });

            // Act
            await command.ExecuteAsync(argsWithoutPro);
            await command.ExecuteAsync(argsWithPro);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("Free Display") && !state.ToString()!.Contains("Commercial (Pro)")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString()!.Contains("Free Display") &&
                        state.ToString()!.Contains("Commercial (Pro)") &&
                        state.ToString()!.Contains("Pro Display")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            telemetryMock.Verify(t => t.TrackActivityAsync(ActivityNameConsts.AbpCliCommandsListModules), Times.Exactly(2));
        }

        private static ModuleInfo CreateModuleInfo(string name, string displayName, bool isPro)
        {
            var moduleInfoType = typeof(ModuleInfoProvider)
                .Assembly
                .GetTypes()
                .First(t => t.Name == "ModuleInfo");

            return (ModuleInfo)Activator.CreateInstance(
                moduleInfoType,
                nonPublic: true,
                args: new object?[]
                {
                    name,
                    displayName,
                    null,
                    null,
                    null,
                    null,
                    null,
                    isPro,
                    new List<string>(),
                    false
                })!;
        }

        private sealed class FakeAsyncDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
}
