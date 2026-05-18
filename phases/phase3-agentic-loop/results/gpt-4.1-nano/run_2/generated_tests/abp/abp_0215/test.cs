using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        private class DummyCommand : ProjectCreationCommandBase
        {
            public DummyCommand() : base(
                null, null, null, null, null, null, null, null, null, null, null, null)
            {
            }

            public override Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_Should_Log_Version_When_Version_Is_Present()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var command = new DummyCommand
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Options = new OptionCollection
                {
                    { Options.Version.Short, "1.0.0" }
                }
            };

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Version: 1.0.0"))),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_Should_Log_Preview_When_Preview_Is_Present()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var command = new DummyCommand
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Options = new OptionCollection
                {
                    { Options.Preview.Long, "true" }
                }
            };

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            mockLogger.Verify(
                x => x.LogInformation("Preview: yes"),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_Should_Log_Pwa_When_Pwa_Is_Present()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var command = new DummyCommand
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Options = new OptionCollection
                {
                    { Options.ProgressiveWebApp.Short, "true" }
                }
            };

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            mockLogger.Verify(
                x => x.LogInformation("Progressive Web App: yes"),
                Times.Once);
        }
    }
}
