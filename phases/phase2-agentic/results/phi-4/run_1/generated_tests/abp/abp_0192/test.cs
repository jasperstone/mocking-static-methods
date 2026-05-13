using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Options;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformation_WhenCommandExists()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<HelpCommand>>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockCommand = new Mock<IConsoleCommand>();

            var commandType = typeof(MockCommand);
            var commandName = commandType.Name;

            var options = new AbpCliOptions();
            options.AddCommand<MockCommand>();

            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService(commandType)).Returns(mockCommand.Object);

            mockScopeFactory.Setup(sf => sf.CreateScope()).Returns(mockScope.Object);

            var helpCommand = new HelpCommand(
                new Options.AbpCliOptionsWrapper(options),
                mockScopeFactory.Object)
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs(target: commandName);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("MockCommand usage info")),
                    It.IsAny<Exception>()),
                Times.Once);
        }

        private class MockCommand : IConsoleCommand
        {
            public Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                return Task.CompletedTask;
            }

            public string GetUsageInfo()
            {
                return "MockCommand usage info";
            }
        }
    }
}
