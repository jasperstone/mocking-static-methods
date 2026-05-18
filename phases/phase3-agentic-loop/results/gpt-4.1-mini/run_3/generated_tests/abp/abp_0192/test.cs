using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class HelpCommandTests
    {
        private class DummyCommand : IConsoleCommand
        {
            public Task ExecuteAsync(CommandLineArgs commandLineArgs) => Task.CompletedTask;
            public string GetUsageInfo() => "Dummy command usage info";
            public static string GetShortDescription() => "Dummy command short description";
        }

        private static IOptions<AbpCliOptions> CreateOptionsWithCommands(Dictionary<string, Type> commands)
        {
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var abpCliOptions = new AbpCliOptions();
            foreach (var cmd in commands)
            {
                abpCliOptions.Commands.Add(cmd.Key, cmd.Value);
            }
            optionsMock.Setup(o => o.Value).Returns(abpCliOptions);
            return optionsMock.Object;
        }

        private static IServiceScopeFactory CreateScopeFactory(Type commandType, IConsoleCommand commandInstance)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(commandType)).Returns(commandInstance);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(commandType)).Returns(commandInstance);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            return scopeFactoryMock.Object;
        }

        [Fact]
        public async Task ExecuteAsync_TargetIsNullOrWhitespace_LogsUsageInfo()
        {
            // Arrange
            var options = CreateOptionsWithCommands(new Dictionary<string, Type>());
            var scopeFactory = CreateScopeFactory(typeof(DummyCommand), new DummyCommand());
            var helpCommand = new HelpCommand(options, scopeFactory);

            var loggerMock = new Mock<ILogger<HelpCommand>>();
            helpCommand.Logger = loggerMock.Object;

            var args = new CommandLineArgs(command: "help", target: null);

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usage:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_TargetNotFound_LogsWarningAndUsageInfo()
        {
            // Arrange
            var commands = new Dictionary<string, Type> { { "existing", typeof(DummyCommand) } };
            var options = CreateOptionsWithCommands(commands);
            var scopeFactory = CreateScopeFactory(typeof(DummyCommand), new DummyCommand());
            var helpCommand = new HelpCommand(options, scopeFactory);

            var loggerMock = new Mock<ILogger<HelpCommand>>();
            helpCommand.Logger = loggerMock.Object;

            var args = new CommandLineArgs(command: "help", target: "nonexistent");

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("There is no command named nonexistent.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usage:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_TargetFound_LogsCommandUsageInfo()
        {
            // Arrange
            var commands = new Dictionary<string, Type> { { "dummy", typeof(DummyCommand) } };
            var options = CreateOptionsWithCommands(commands);
            var dummyCommand = new DummyCommand();
            var scopeFactory = CreateScopeFactory(typeof(DummyCommand), dummyCommand);
            var helpCommand = new HelpCommand(options, scopeFactory);

            var loggerMock = new Mock<ILogger<HelpCommand>>();
            helpCommand.Logger = loggerMock.Object;

            var args = new CommandLineArgs(command: "help", target: "dummy");

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == dummyCommand.GetUsageInfo()),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
