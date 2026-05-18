using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Xunit;

public class CleanCommandTests
{
    [Fact]
    public async Task ExecuteAsync_LogsInformationMessageForBinAndObjFoldersRemoved()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CleanCommand>>();
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockFileSystem = new MockFileSystem();
        var mockTelemetryService = new Mock<ITelemetryService>();

        var cleanCommand = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object, mockFileSystem)
        {
            Logger = mockLogger.Object
        };

        // Act
        await cleanCommand.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation($"'bin' and 'obj' folders removed successfully!"),
            Times.Once);
    }
}
