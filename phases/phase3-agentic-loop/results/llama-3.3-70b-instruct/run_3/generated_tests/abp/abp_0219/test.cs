using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsConnectionString_WhenConnectionStringIsProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectCreationCommandBase>>();
            var commandBase = new ProjectCreationCommand(
                null, null, null, null, null, null, null, null, null, null, null, null, null);
            commandBase.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("--connection-string", "connection-string");

            // Act
            await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "", "");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Connection string: connection-string"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_DoesNotLogConnectionString_WhenConnectionStringIsNotProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectCreationCommandBase>>();
            var commandBase = new ProjectCreationCommand(
                null, null, null, null, null, null, null, null, null, null, null, null, null);
            commandBase.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs();

            // Act
            await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "", "");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Never);
        }
    }
}
