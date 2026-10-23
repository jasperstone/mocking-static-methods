using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WithValidTarget_ShouldLogCommandUsageInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var commandMock = new Mock<IConsoleCommand>();

            var commandsDict = new Dictionary<string, Type>
            {
                { "test", typeof(TestCommand) }
            };

            var options = new AbpCliOptions
            {
                Commands = commandsDict
            };

            var cliOptions = Options.Create(options);

            // Setup command mock to return usage info
            commandMock.Setup(c => c.GetUsageInfo()).Returns("Usage info for test command");

            // Setup service provider to return the command mock
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => commandMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider);
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            var helpCommand = new HelpCommand(cliOptions, scopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            var args = new CommandLineArgs { Target = "test" };
            await helpCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("Usage info for test command"),
                Times.Once);
        }

        // Dummy command class for testing
        public class TestCommand : IConsoleCommand
        {
            public Task ExecuteAsync(CommandLineArgs args) => Task.CompletedTask;
            public string GetUsageInfo() => "Usage info for test command";
        }
    }
}
