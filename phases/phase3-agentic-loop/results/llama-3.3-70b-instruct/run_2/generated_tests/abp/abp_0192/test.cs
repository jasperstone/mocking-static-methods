using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var cliOptions = new AbpCliOptions();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var helpCommand = new HelpCommand(Mock.Of<IOptions<AbpCliOptions>>(), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;

            // Act
            await helpCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsUnknown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var cliOptions = new AbpCliOptions();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var helpCommand = new HelpCommand(Mock.Of<IOptions<AbpCliOptions>>(), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;

            // Act
            await helpCommand.ExecuteAsync(new CommandLineArgs { Command = "Unknown" });

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ExecuteAsync_LogsCommandUsageInfo_WhenTargetIsKnown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var cliOptions = new AbpCliOptions();
            cliOptions.Commands.Add("Known", typeof(MockCommand));
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(s => s.CreateScope()).Returns(new Mock<IServiceScope>().Object);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(It.IsAny<Type>())).Returns(new MockCommand());
            serviceScopeFactoryMock.Setup(s => s.CreateScope()).Returns(new Mock<IServiceScope>().SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object).Object);
            var helpCommand = new HelpCommand(Mock.Of<IOptions<AbpCliOptions>>(), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;

            // Act
            await helpCommand.ExecuteAsync(new CommandLineArgs { Command = "Known" });

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Exactly(2));
        }

        private class MockCommand : IConsoleCommand
        {
            public Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                return Task.CompletedTask;
            }

            public string GetUsageInfo()
            {
                return "Mock usage info";
            }
        }
    }
}
