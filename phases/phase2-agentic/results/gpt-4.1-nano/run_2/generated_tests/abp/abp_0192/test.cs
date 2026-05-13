using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Moq;

namespace Volo.Abp.Cli.Tests
{
    public class HelpCommandTests
    {
        private readonly Mock<ILogger<HelpCommand>> _loggerMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IConsoleCommand> _consoleCommandMock;
        private readonly HelpCommand _helpCommand;
        private readonly AbpCliOptions _cliOptions;

        public HelpCommandTests()
        {
            _loggerMock = new Mock<ILogger<HelpCommand>>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _consoleCommandMock = new Mock<IConsoleCommand>();

            _scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);

            var options = Options.Create(new AbpCliOptions
            {
                Commands = new System.Collections.Generic.Dictionary<string, Type>
                {
                    { "test", typeof(TestCommand) }
                }
            });

            _helpCommand = new HelpCommand(options, _scopeFactoryMock.Object);
            _helpCommand.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_When_TargetIsNullOrWhitespace()
        {
            // Arrange
            var args = new CommandLineArgs { Target = "   " };

            // Act
            await _helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogWarningAndInformation_When_CommandNotFound()
        {
            // Arrange
            var args = new CommandLineArgs { Target = "unknown" };
            _helpCommand.AbpCliOptions.Commands.Clear();

            // Act
            await _helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.LogWarning(It.Is<string>(s => s.Contains("There is no command named"))), Times.Once);
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogUsageInfo_When_CommandExists()
        {
            // Arrange
            var args = new CommandLineArgs { Target = "test" };
            _serviceProviderMock.Setup(sp => sp.GetRequiredService(It.IsAny<Type>())).Returns(new TestCommand());

            // Act
            await _helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Usage"))), Times.Once);
        }

        // Dummy command class for testing
        public class TestCommand : IConsoleCommand
        {
            public string GetUsageInfo() => "Test Command Usage";
        }
    }
}
