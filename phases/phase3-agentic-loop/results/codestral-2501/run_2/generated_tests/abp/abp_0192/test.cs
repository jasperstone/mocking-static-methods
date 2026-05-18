using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Internal;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Commands
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WithNullOrWhiteSpaceTarget_LogsUsageInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var commandLineArgs = new CommandLineArgs(null, " ");

            var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithUnknownCommand_LogsWarningAndUsageInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var commandLineArgs = new CommandLineArgs(null, "unknown");

            var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidCommand_LogsCommandUsageInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var commandLineArgs = new CommandLineArgs(null, "valid");

            var commandMock = new Mock<IConsoleCommand>();
            commandMock.Setup(x => x.GetUsageInfo()).Returns("Usage info");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService(typeof(IConsoleCommand)))
                .Returns(commandMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            var commands = new Dictionary<string, Type>
            {
                { "valid", typeof(IConsoleCommand) }
            };

            var abpCliOptions = new AbpCliOptions();
            abpCliOptions.Commands = commands;

            optionsMock.Setup(x => x.Value).Returns(abpCliOptions);

            var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
