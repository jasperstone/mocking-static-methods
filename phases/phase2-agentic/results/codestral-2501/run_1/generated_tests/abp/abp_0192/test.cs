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
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class HelpCommandTests
    {
        private readonly Mock<ILogger<HelpCommand>> _loggerMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly HelpCommand _helpCommand;

        public HelpCommandTests()
        {
            _loggerMock = new Mock<ILogger<HelpCommand>>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceScopeMock = new Mock<IServiceScope>();

            var cliOptions = new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>
                {
                    { "test", typeof(TestCommand) }
                }
            };

            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(cliOptions);

            _serviceScopeFactoryMock.Setup(s => s.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);

            _helpCommand = new HelpCommand(optionsMock.Object, _serviceScopeFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_WithValidCommand_LogsUsageInfo()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("help", "test");

            _serviceProviderMock.Setup(s => s.GetRequiredService(typeof(TestCommand)))
                .Returns(new TestCommand());

            // Act
            await _helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidCommand_LogsWarningAndUsageInfo()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("help", "invalid");

            // Act
            await _helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithNoTarget_LogsUsageInfo()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("help");

            // Act
            await _helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        private class TestCommand : IConsoleCommand, ITransientDependency
        {
            public Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                return Task.CompletedTask;
            }

            public string GetUsageInfo()
            {
                return "Test command usage info";
            }
        }
    }
}
