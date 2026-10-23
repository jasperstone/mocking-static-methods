using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Events;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Internal.Telemetry.Constants.Enums;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        private readonly Mock<ILogger<NewCommand>> _loggerMock;
        private readonly Mock<ProjectCreationCommandBase> _commandBaseMock;

        public ProjectCreationCommandBaseTests()
        {
            _loggerMock = new Mock<ILogger<NewCommand>>();
            _commandBaseMock = new Mock<ProjectCreationCommandBase>(
                Mock.Of<ConnectionStringProvider>(),
                Mock.Of<SolutionPackageVersionFinder>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<IInstallLibsService>(),
                Mock.Of<CliService>(),
                Mock.Of<AngularPwaSupportAdder>(),
                Mock.Of<InitialMigrationCreator>(),
                Mock.Of<ThemePackageAdder>(),
                Mock.Of<ILocalEventBus>(),
                Mock.Of<IBundlingService>(),
                Mock.Of<AngularThemeConfigurer>(),
                Mock.Of<CliVersionService>()
            );
            _commandBaseMock.Setup(x => x.Logger).Returns(_loggerMock.Object);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogVersion_WhenVersionIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Options = new AbpCommandLineOptions
                {
                    { "v", "1.0.0" }
                }
            };

            // Act
            await _commandBaseMock.Object.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Version: 1.0.0")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogPreview_WhenPreviewIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Options = new AbpCommandLineOptions
                {
                    { "--preview", "true" }
                }
            };

            // Act
            await _commandBaseMock.Object.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Preview: yes")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogPwa_WhenPwaIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Options = new AbpCommandLineOptions
                {
                    { "-p", "true" }
                }
            };

            // Act
            await _commandBaseMock.Object.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Progressive Web App: yes")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogDatabaseProvider_WhenDatabaseProviderIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Options = new AbpCommandLineOptions
                {
                    { "-d", "SqlServer" }
                }
            };

            // Act
            await _commandBaseMock.Object.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database provider: SqlServer")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
