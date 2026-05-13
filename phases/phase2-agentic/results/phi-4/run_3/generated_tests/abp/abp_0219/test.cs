using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsConnectionString_WhenProvided()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProjectCreationCommandBase>>();
            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { "ConnectionString", "TestConnectionString" }
                }
            };
            var command = new Mock<ProjectCreationCommandBase>(null, null, null, null, null, null, null, null, null, null, null, null, null, mockLogger.Object)
            {
                CallBase = true
            }.Object;

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(It.Is<string>(s => s.Contains("Connection string: TestConnectionString"))),
                Times.Once);
        }
    }
}
