using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class HelpCommandTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ILogger<HelpCommand>> _loggerMock;
        private readonly HelpCommand _helpCommand;
        private readonly AbpCliOptions _cliOptions;

        public HelpCommandTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _loggerMock = new Mock<ILogger<HelpCommand>>();

            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);

            _cliOptions = new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>
                {
                    { "test", typeof(TestCommand) }
                }
            };

            _helpCommand = new HelpCommand(
                Options.Create(_cliOptions),
                _serviceScopeFactoryMock.Object
            );
            _helpCommand.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_When_TargetIsNullOrWhitespace()
        {
            // Arrange
            var args = new CommandLineArgs(null, "   ");

            // Act
            await _helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogWarningAndInformation_When_CommandNotFound()
        {
            // Arrange
            var args = new CommandLineArgs(null, "unknown");

            // Act
            await _helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.LogWarning(It.Is<string>(s => s.Contains("There is no command named"))), Times.Once);
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_WithCommandUsageInfo_When_CommandExists()
        {
            // Arrange
            var args = new CommandLineArgs(null, "test");

            var commandInstance = new TestCommand();

            _serviceProviderMock.Setup(sp => sp.GetRequiredService(It.IsAny<Type>())).Returns(commandInstance);

            // Act
            await _helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Usage:"))), Times.Once);
        }

        [Fact]
        public void GetUsageInfo_Should_ReturnStringWithUsageAndCommands()
        {
            // Arrange
            var options = new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>
                {
                    { "test", typeof(TestCommand) }
                }
            };
            var helpCmd = new HelpCommand(Options.Create(options), _serviceScopeFactoryMock.Object);
            var result = helpCmd.GetUsageInfo();

            // Act & Assert
            Assert.Contains("Usage:", result);
            Assert.Contains("Command List:", result);
            Assert.Contains("test", result);
        }

        // Dummy command class for testing
        public class TestCommand : IConsoleCommand
        {
            public Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                return Task.CompletedTask;
            }

            public string GetUsageInfo() => "Test command usage info";
        }
    }
}
