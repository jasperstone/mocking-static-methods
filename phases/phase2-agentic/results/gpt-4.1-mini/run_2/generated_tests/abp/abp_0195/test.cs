using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsExpectedOutput_WithAndWithoutProModules()
        {
            // Arrange
            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>(null!, null!, null!, null!);
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var telemetryActivityMock = new Mock<IAsyncDisposable>();
            telemetryServiceMock
                .Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
                .ReturnsAsync(telemetryActivityMock.Object);

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var freeModule = new ModuleInfo { Name = "FreeModule", DisplayName = "Free Module", IsPro = false };
            var proModule = new ModuleInfo { Name = "ProModule", DisplayName = "Pro Module", IsPro = true };

            moduleInfoProviderMock
                .Setup(m => m.GetModuleListAsync())
                .ReturnsAsync(new List<ModuleInfo> { freeModule, proModule });

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act - without pro modules
            var argsWithoutPro = new CommandLineArgs(new string[0]);
            await command.ExecuteAsync(argsWithoutPro);

            // Assert - verify log contains free module but not pro module
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Open Source Application Modules") && v.ToString()!.Contains(freeModule.DisplayName) && !v.ToString()!.Contains(proModule.DisplayName)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Reset();

            // Act - with pro modules
            var argsWithPro = new CommandLineArgs(new[] { "--include-pro-modules" });
            await command.ExecuteAsync(argsWithPro);

            // Assert - verify log contains both free and pro modules sections
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Open Source Application Modules") && v.ToString()!.Contains(freeModule.DisplayName) && v.ToString()!.Contains("Commercial (Pro) Application Modules") && v.ToString()!.Contains(proModule.DisplayName)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Minimal ModuleInfo class for testing
    public class ModuleInfo
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public bool IsPro { get; set; }
    }
}
