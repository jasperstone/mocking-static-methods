using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class HelpCommandTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IConsoleCommand> _consoleCommandMock;
        private readonly Mock<ILogger<HelpCommand>> _loggerMock;
        private readonly AbpCliOptions _cliOptions;

        public HelpCommandTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _consoleCommandMock = new Mock<IConsoleCommand>();
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
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_When_TargetIsNullOrWhitespace()
        {
            // Arrange
            var helpCommand = new HelpCommand(
                Options.Create(_cliOptions),
                _serviceScopeFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };

            var args = new CommandLineArgs { Target = "   " };

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogWarningAndInformation_When_CommandNotFound()
        {
            // Arrange
            var helpCommand = new HelpCommand(
                Options.Create(_cliOptions),
                _serviceScopeFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };

            var args = new CommandLineArgs { Target = "unknown" };

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.LogWarning(It.Is<string>(s => s.Contains("There is no command named"))), Times.Once);
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogUsageInfo_When_CommandFound()
        {
            // Arrange
            var helpCommand = new HelpCommand(
                Options.Create(_cliOptions),
                _serviceScopeFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };

            var args = new CommandLineArgs { Target = "test" };

            _serviceProviderMock.Setup(sp => sp.GetRequiredService(It.IsAny<Type>()))
                .Returns(_consoleCommandMock.Object);

            _consoleCommandMock.Setup(c => c.GetUsageInfo()).Returns("usage info");

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.LogInformation("usage info"), Times.Once);
        }

        [Fact]
        public void GetUsageInfo_Should_ReturnStringWithExpectedContent()
        {
            // Arrange
            var helpCommand = new HelpCommand(
                Options.Create(_cliOptions),
                _serviceScopeFactoryMock.Object);

            // Act
            var result = helpCommand.GetUsageInfo();

            // Assert
            Assert.Contains("Usage:", result);
            Assert.Contains("abp <command> <target> [options]", result);
            Assert.Contains("Command List:", result);
            Assert.Contains("> test", result);
        }

        // Dummy command class for testing
        public class TestCommand : IConsoleCommand
        {
            public string GetUsageInfo() => "test usage info";
        }
    }
}
