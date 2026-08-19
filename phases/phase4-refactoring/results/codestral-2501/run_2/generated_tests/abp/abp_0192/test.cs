using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Internal;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WithNullOrWhiteSpaceTarget_LogsUsageInfo()
        {
            // Arrange
            var cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var loggerMock = new Mock<ILogger<HelpCommand>>();

            var helpCommand = new HelpCommand(cliOptionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs(target: "  ");

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithUnknownCommand_LogsWarningAndUsageInfo()
        {
            // Arrange
            var cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var loggerMock = new Mock<ILogger<HelpCommand>>();

            var helpCommand = new HelpCommand(cliOptionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs(target: "unknown");

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>()),
                Times.Once);
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidCommand_LogsCommandUsageInfo()
        {
            // Arrange
            var cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var loggerMock = new Mock<ILogger<HelpCommand>>();

            var commandMock = new Mock<IConsoleCommand>();
            commandMock.Setup(x => x.GetUsageInfo()).Returns("Command usage info");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService(It.IsAny<Type>())).Returns(commandMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            var helpCommand = new HelpCommand(cliOptionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs(target: "valid");

            var abpCliOptions = new Mock<AbpCliOptions>();
            abpCliOptions.Setup(x => x.Commands).Returns(new Dictionary<string, Type>
            {
                { "valid", typeof(IConsoleCommand) }
            });

            cliOptionsMock.Setup(x => x.Value).Returns(abpCliOptions.Object);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Command usage info"),
                Times.Once);
        }
    }
}
