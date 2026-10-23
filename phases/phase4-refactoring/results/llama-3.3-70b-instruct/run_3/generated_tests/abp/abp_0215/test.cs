using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Options;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsVersion_WhenVersionIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectCreationCommandBase>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Add(Options.Version.Short, "1.0.0");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Version: 1.0.0"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsPreview_WhenPreviewIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectCreationCommandBase>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Add(Options.Preview.Long, "true");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Preview: yes"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsPwa_WhenPwaIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectCreationCommandBase>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Add(Options.ProgressiveWebApp.Short, "true");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Progressive Web App: yes"), Times.Once);
        }
    }
}
