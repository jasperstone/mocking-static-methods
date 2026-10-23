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
            var cliOptions = Options.Create(new AbpCliOptions());
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var loggerMock = new Mock<ILogger<HelpCommand>>();

            var helpCommand = new HelpCommand(cliOptions, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs(" ", new List<string>());

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
            var cliOptions = Options.Create(new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>()
            });
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var loggerMock = new Mock<ILogger<HelpCommand>>();

            var helpCommand = new HelpCommand(cliOptions, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs("unknown", new List<string>());

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithKnownCommand_LogsCommandUsageInfo()
        {
            // Arrange
            var cliOptions = Options.Create(new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>
                {
                    { "known", typeof(MockConsoleCommand) }
                }
            });
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<HelpCommand>>();

            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService(typeof(MockConsoleCommand)))
                .Returns(new MockConsoleCommand());

            var helpCommand = new HelpCommand(cliOptions, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs("known", new List<string>());

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }

    public class MockConsoleCommand : IConsoleCommand
    {
        public Task ExecuteAsync(CommandLineArgs commandLineArgs)
        {
            return Task.CompletedTask;
        }

        public string GetUsageInfo()
        {
            return "Mock usage info";
        }
    }
}
