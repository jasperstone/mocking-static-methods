using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class HelpCommandTests
    {
        private readonly Mock<IOptions<AbpCliOptions>> _cliOptionsMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<ILogger<HelpCommand>> _loggerMock;
        private readonly HelpCommand _helpCommand;

        public HelpCommandTests()
        {
            _cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _loggerMock = new Mock<ILogger<HelpCommand>>();

            var abpCliOptions = new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
                {
                    { "test", typeof(TestCommand) }
                }
            };
            _cliOptionsMock.Setup(x => x.Value).Returns(abpCliOptions);

            _helpCommand = new HelpCommand(_cliOptionsMock.Object, _serviceScopeFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_WithValidCommand_LogsUsageInfo()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs { Target = "test" };
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var testCommandMock = new Mock<IConsoleCommand>();

            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService(typeof(TestCommand))).Returns(testCommandMock.Object);

            // Act
            await _helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidCommand_LogsWarningAndUsageInfo()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs { Target = "invalid" };

            // Act
            await _helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Once);
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithNoTarget_LogsUsageInfo()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs { Target = "" };

            // Act
            await _helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        public class TestCommand : IConsoleCommand
        {
            public Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                throw new NotImplementedException();
            }

            public string GetUsageInfo()
            {
                return "Test command usage info";
            }
        }
    }
}
