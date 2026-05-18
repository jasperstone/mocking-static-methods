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

public class HelpCommandTests
{
    private readonly Mock<ILogger<HelpCommand>> _loggerMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly HelpCommand _helpCommand;
    private readonly AbpCliOptions _options;

    public HelpCommandTests()
    {
        _loggerMock = new Mock<ILogger<HelpCommand>>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);

        var options = new AbpCliOptions
        {
            Commands = new Dictionary<string, Type>()
        };
        _options = options;

        _helpCommand = new HelpCommand(
            Options.Create(_options),
            _scopeFactoryMock.Object
        );
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
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogWarningAndInformation_When_CommandNotFound()
    {
        // Arrange
        var args = new CommandLineArgs { Target = "nonexistent" };
        _options.Commands = new Dictionary<string, Type>();

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
        var commandType = typeof(MockCommand);
        _options.Commands = new Dictionary<string, Type>
        {
            { "test", commandType }
        };

        _serviceProviderMock.Setup(sp => sp.GetRequiredService(commandType))
            .Returns(new MockCommand());

        var args = new CommandLineArgs { Target = "test" };

        // Act
        await _helpCommand.ExecuteAsync(args);

        // Assert
        _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Usage"))), Times.Once);
    }

    // Mock command class for testing
    public class MockCommand : IConsoleCommand
    {
        public string GetUsageInfo() => "Mock usage info";

        public Task ExecuteAsync(CommandLineArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
