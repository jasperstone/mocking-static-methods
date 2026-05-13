using Xunit;
using Moq;
using Volo.Abp.Cli.Core;
using Volo.Abp.Cli.Args;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public void GetProjectBuildArgsAsync_LogsConnectionString_WhenConnectionStringIsProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs(new[] { "-cs", "connection-string" });
            var projectCreationCommandBase = new ProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);

            // Act
            projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "project-name");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Connection string: connection-string"), Times.Once);
        }

        [Fact]
        public void GetProjectBuildArgsAsync_DoesNotLogConnectionString_WhenConnectionStringIsNotProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs(new string[0]);
            var projectCreationCommandBase = new ProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);

            // Act
            projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "project-name");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Never);
        }
    }
}
