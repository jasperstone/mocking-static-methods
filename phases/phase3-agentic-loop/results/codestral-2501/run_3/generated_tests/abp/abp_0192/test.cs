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
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WithNoTarget_LogsUsageInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var commandLineArgs = new CommandLineArgs();

            var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithUnknownTarget_LogsWarningAndUsageInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var commandLineArgs = new CommandLineArgs("unknown");

            var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

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
        public async Task ExecuteAsync_WithKnownTarget_LogsCommandUsageInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var commandLineArgs = new CommandLineArgs("known");

            var commandMock = new Mock<IConsoleCommand>();
            commandMock.Setup(x => x.GetUsageInfo()).Returns("Usage info for known command");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService(typeof(IConsoleCommand))).Returns(commandMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            var commands = new Dictionary<string, Type>
            {
                { "known", typeof(IConsoleCommand) }
            };
            var abpCliOptions = new Mock<AbpCliOptions>();
            abpCliOptions.Setup(x => x.Commands).Returns(commands);
            optionsMock.Setup(x => x.Value).Returns(abpCliOptions.Object);

            var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Usage info for known command"),
                Times.Once);
        }
    }
}
