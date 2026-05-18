using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogInformationIsCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>();
            var newCommand = new NewCommand(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, templateProjectBuilderMock.Object, loggerMock.Object);

            var commandLineArgs = new CommandLineArgs("new", "MyProject");
            commandLineArgs.Options.Add("--template", "app");
            commandLineArgs.Options.Add("--tiered", "");

            // Act
            await newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Tiered: yes"), Times.Once);
        }
    }
}
