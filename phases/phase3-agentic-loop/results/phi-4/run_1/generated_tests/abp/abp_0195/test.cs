using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

public class ListModulesCommandTests
{
    [Fact]
    public async Task ExecuteAsync_LogsCorrectInformation()
    {
        // Arrange
        var mockModuleInfoProvider = new Mock<ModuleInfoProvider>();
        var mockTelemetryService = new Mock<ITelemetryService>();
        var mockLogger = new Mock<ILogger<ListModulesCommand>>();

        var freeModules = new List<ModuleInfo>
        {
            new ModuleInfo { DisplayName = "Free Module 1", Name = "FreeModule1", IsPro = false },
            new ModuleInfo { DisplayName = "Free Module 2", Name = "FreeModule2", IsPro = false }
        };

        var proModules = new List<ModuleInfo>
        {
            new ModuleInfo { DisplayName = "Pro Module 1", Name = "ProModule1", IsPro = true }
        };

        mockModuleInfoProvider
            .Setup(m => m.GetModuleListAsync())
            .ReturnsAsync(freeModules.Concat(proModules).ToList());

        var command = new ListModulesCommand(mockModuleInfoProvider.Object, mockTelemetryService.Object)
        {
            Logger = mockLogger.Object
        };

        var commandLineArgs = new CommandLineArgs
        {
            Options = new Dictionary<string, string> { { "include-pro-modules", string.Empty } }
        };

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        var expectedOutput = new StringBuilder(Environment.NewLine)
            .AppendLine("Open Source Application Modules")
            .AppendLine()
            .AppendLine($"> {freeModules[0].DisplayName.PadRight(50)} ({freeModules[0].Name})")
            .AppendLine($"> {freeModules[1].DisplayName.PadRight(50)} ({freeModules[1].Name})")
            .AppendLine()
            .AppendLine("Commercial (Pro) Application Modules")
            .AppendLine()
            .AppendLine($"> {proModules[0].DisplayName.PadRight(50)} ({proModules[0].Name})")
            .AppendLine();

        mockLogger.Verify(
            logger => logger.LogInformation(expectedOutput.ToString()),
            Times.Once);
    }
}
