using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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
            var mockCommandType = typeof(MockCommand);
            var mockCommand = new Mock<IConsoleCommand>();
            mockCommand.Setup(c => c.GetUsageInfo()).Returns("Mock command usage info");

            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService(mockCommandType)).Returns(mockCommand.Object);

            mockScopeFactory.Setup(sf => sf.CreateScope()).Returns(mockScope.Object);

            var mockOptions = new Mock<IOptions<AbpCliOptions>>();
            var abpCliOptions = new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>
                {
                    { "mock", mockCommandType }
                }
            };
            mockOptions.Setup(o => o.Value).Returns(abpCliOptions);

            var helpCommand = new HelpCommand(mockOptions.Object, mockScopeFactory.Object)
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Target = "mock"
            };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("Mock command usage info"),
                Times.Once);
        }

        private class MockCommand : IConsoleCommand
        {
            public string GetUsageInfo()
            {
                return "Mock command usage info";
            }
        }
    }
}
