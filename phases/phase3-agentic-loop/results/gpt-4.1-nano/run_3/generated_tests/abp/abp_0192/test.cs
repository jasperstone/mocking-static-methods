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

        public HelpCommandTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _loggerMock = new Mock<ILogger<HelpCommand>>();

            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_When_TargetIsNullOrWhitespace()
        {
            // Arrange
            var options = Options.Create(new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>()
            });
            var helpCommand = new HelpCommand(options, _serviceScopeFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };
            var args = new CommandLineArgs { Target = "   " };

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogWarningAndInformation_When_CommandDoesNotExist()
        {
            // Arrange
            var options = Options.Create(new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>()
            });
            var helpCommand = new HelpCommand(options, _serviceScopeFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };
            var args = new CommandLineArgs { Target = "nonexistent" };

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.LogWarning(It.Is<string>(s => s.Contains("There is no command named"))), Times.Once);
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_WithCommandUsageInfo_When_CommandExists()
        {
            // Arrange
            var commandType = typeof(TestCommand);
            var options = Options.Create(new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>
                {
                    { "test", commandType }
                }
            });
            var helpCommand = new HelpCommand(options, _serviceScopeFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };

            var commandInstance = new TestCommand();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(commandType)).Returns(commandInstance);
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var args = new CommandLineArgs { Target = "test" };

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Usage:"))), Times.Once);
        }

        // Dummy command class for testing
        public class TestCommand : IConsoleCommand
        {
            public string GetUsageInfo() => "Test command usage info.";
            public Task ExecuteAsync(CommandLineArgs args) => Task.CompletedTask;
        }
    }
}
