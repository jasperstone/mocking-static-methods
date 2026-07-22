using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class HelpCommandTests
    {
        private readonly Mock<ILogger<HelpCommand>> _loggerMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IConsoleCommand> _consoleCommandMock;
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

            _cliOptions = new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>
                {
                    { "test", typeof(TestCommand) }
                }
            };
        }

        [Fact]
        public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsNullOrWhitespace()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(_cliOptions);
            var helpCommand = new HelpCommand(optionsMock.Object, _scopeFactoryMock.Object);
            helpCommand.Logger = _loggerMock.Object;

            var args = new CommandLineArgs { Target = "  " };

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usage:")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsWarningAndUsage_WhenCommandNotFound()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(_cliOptions);
            var helpCommand = new HelpCommand(optionsMock.Object, _scopeFactoryMock.Object);
            helpCommand.Logger = _loggerMock.Object;

            var args = new CommandLineArgs { Target = "unknown" };

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("There is no command named")),
                null,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);

            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usage:")),
                null,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsCommandUsage_WhenCommandExists()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(_cliOptions);
            var helpCommand = new HelpCommand(optionsMock.Object, _scopeFactoryMock.Object);
            helpCommand.Logger = _loggerMock.Object;

            var args = new CommandLineArgs { Target = "test" };

            _serviceProviderMock.Setup(sp => sp.GetRequiredService(It.IsAny<Type>()))
                .Returns(_consoleCommandMock.Object);

            _consoleCommandMock.Setup(c => c.GetUsageInfo()).Returns("usage info");

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("usage info")),
                null,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }

    // Dummy command class for testing
    public class TestCommand : IConsoleCommand
    {
        public string GetUsageInfo() => "usage info";

        public Task ExecuteAsync(CommandLineArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
