using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
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
            var commandLineArgs = new CommandLineArgs
            {
                Options = new CommandLineOptions
                {
                    { "v", "1.0.0" }
                }
            };
            var projectCreationCommandBase = new ProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);

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
            var commandLineArgs = new CommandLineArgs
            {
                Options = new CommandLineOptions
                {
                    { "--preview", true }
                }
            };
            var projectCreationCommandBase = new ProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);

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
            var commandLineArgs = new CommandLineArgs
            {
                Options = new CommandLineOptions
                {
                    { "-p", true }
                }
            };
            var projectCreationCommandBase = new ProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Progressive Web App: yes"), Times.Once);
        }
    }
}
