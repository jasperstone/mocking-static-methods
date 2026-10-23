using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaChatCompletion_ServiceProvider_GetService_ReturnsLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

        // Assert
        Assert.NotNull(loggerFactory);
    }

    [Fact]
    public void AddOllamaChatCompletion_ServiceProvider_GetService_ReturnsNullWhenNoLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

        // Assert
        Assert.Null(loggerFactory);
    }
}
