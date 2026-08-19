using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogUsageInfo_When_TargetIsNullOrWhitespace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<HelpCommand>>();
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceScope = new Mock<IServiceScope>();
            var options = new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>()
            };
            var mockOptions = new Mock<IOptions<AbpCliOptions>>();
            mockOptions.Setup(o => o.Value).Returns(options);

            var helpCommand = new HelpCommand(mockOptions.Object, mockServiceScopeFactory.Object)
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs { Target = null };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogWarningAndUsageInfo_When_CommandNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<HelpCommand>>();
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceScope = new Mock<IServiceScope>();
            var options = new AbpCliOptions
            {
                Commands = new Dictionary<string, Type> { { "test", typeof(MockCommand) } }
            };
            var mockOptions = new Mock<IOptions<AbpCliOptions>>();
            mockOptions.Setup(o => o.Value).Returns(options);

            var helpCommand = new HelpCommand(mockOptions.Object, mockServiceScopeFactory.Object)
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs { Target = "nonexistent" };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("There is no command named"))), Times.Once);
            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogCommandUsageInfo_When_CommandExists()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<HelpCommand>>();
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockCommand = new Mock<IConsoleCommand>();
            mockCommand.Setup(c => c.GetUsageInfo()).Returns("usage info");
            var commandType = typeof(MockCommand);
            var commands = new Dictionary<string, Type> { { "test", commandType } };
            var options = new AbpCliOptions { Commands = commands };
            var mockOptions = new Mock<IOptions<AbpCliOptions>>();
            mockOptions.Setup(o => o.Value).Returns(options);

            var helpCommand = new HelpCommand(mockOptions.Object, mockServiceScopeFactory.Object)
            {
                Logger = mockLogger.Object
            };

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(commandType)).Returns(mockCommand.Object);
            mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

            var commandLineArgs = new CommandLineArgs { Target = "test" };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(l => l.LogInformation("usage info"), Times.Once);
        }

        // Mock command class for testing
        public class MockCommand : IConsoleCommand
        {
            public string GetUsageInfo() => "usage info";

            public Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                throw new NotImplementedException();
            }
        }
    }
}
