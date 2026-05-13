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

namespace Volo.Abp.Cli.Tests
{
    public class HelpCommandTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IConsoleCommand> _consoleCommandMock;
        private readonly Mock<ILogger<HelpCommand>> _loggerMock;
        private readonly HelpCommand _helpCommand;
        private readonly AbpCliOptions _abpCliOptions;

        public HelpCommandTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _consoleCommandMock = new Mock<IConsoleCommand>();
            _loggerMock = new Mock<ILogger<HelpCommand>>();

            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);

            var commandsDict = new Dictionary<string, Type>
            {
                { "test", typeof(TestCommand) }
            };

            _abpCliOptions = new AbpCliOptions
            {
                Commands = commandsDict
            };

            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(_abpCliOptions);

            _helpCommand = new HelpCommand(optionsMock.Object, _serviceScopeFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_When_TargetIsNullOrWhitespace()
        {
            // Arrange
            var args = new CommandLineArgs { Target = "   " };

            // Act
            await _helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(
                l => l.LogInformation(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogWarningAndInformation_When_CommandNotFound()
        {
            // Arrange
            var args = new CommandLineArgs { Target = "unknown" };

            // Act
            await _helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(
                l => l.LogWarning(It.Is<string>(s => s.Contains("There is no command named"))),
                Times.Once);
            _loggerMock.Verify(
                l => l.LogInformation(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_WithCommandUsageInfo_When_CommandExists()
        {
            // Arrange
            var args = new CommandLineArgs { Target = "test" };

            var commandInstance = new TestCommand();

            _serviceProviderMock
                .Setup(sp => sp.GetRequiredService(It.IsAny<Type>()))
                .Returns(commandInstance);

            // Act
            await _helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains(commandInstance.GetUsageInfo()))),
                Times.Once);
        }

        [Fact]
        public void GetUsageInfo_Should_ReturnExpectedString()
        {
            // Act
            var usageInfo = _helpCommand.GetUsageInfo();

            // Assert
            Assert.Contains("Usage:", usageInfo);
            Assert.Contains("Command List:", usageInfo);
            Assert.Contains("To get a detailed help for a command:", usageInfo);
        }

        // Dummy command class for testing
        public class TestCommand : IConsoleCommand
        {
            public string GetUsageInfo()
            {
                return "Test Command Usage Info";
            }
        }
    }
}
