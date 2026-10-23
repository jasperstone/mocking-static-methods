using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTest
    {
        private class DummyCommand : ProjectCreationCommandBase
        {
            public DummyCommand(ILogger<NewCommand> logger)
                : base(
                      null, null, null, null, null, null, null, null, null, null, null, null)
            {
                Logger = logger;
            }
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_Should_LogInformation_ForPreviewAndPwa()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new DummyCommand(loggerMock.Object);

            var options = new Dictionary<string, string>
            {
                { Options.Preview.Long, "true" },
                { Options.ProgressiveWebApp.Short, "true" },
                { Options.GitHubAbpLocalRepositoryPath.Long, "/path/to/repo" }
            };
            var commandLineArgs = new CommandLineArgs
            {
                Options = options
            };

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Preview: yes"), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Progressive Web App: yes"), Times.Once);
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.StartsWith("GitHub Abp Local Repository Path:"))), Times.Once);
        }
    }
}
