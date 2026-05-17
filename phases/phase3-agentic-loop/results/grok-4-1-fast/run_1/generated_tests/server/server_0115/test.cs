using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Xunit;
using Bit.SharedWeb.Utilities;
using Moq;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTokenizers_RegistersDuoUserStateTokenFactory_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();

        // Act
        services.AddTokenizers();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IDataProtectorTokenFactory<DuoUserStateTokenable>>();
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddTokenizers_ThrowsWhenLoggerMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDataProtection();

        // Act & Assert
        services.AddTokenizers();
        var serviceProvider = services.BuildServiceProvider();
        
        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetRequiredService<IDataProtectorTokenFactory<DuoUserStateTokenable>>());
        Assert.Contains("Unable to resolve service", exception.Message);
    }

    [Fact]
    public void AddTokenizers_GetRequiredServiceCalledOnLine204_Success()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(new TestLoggerProvider()));
        services.AddDataProtection();

        // Act
        services.AddTokenizers();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - specifically tests the GetRequiredService call on line 204 succeeds
        var duoFactory = serviceProvider.GetRequiredService<IDataProtectorTokenFactory<DuoUserStateTokenable>>();
        Assert.NotNull(duoFactory);
    }

    private class TestLoggerProvider : ILoggerProvider
    {
        public void Dispose() { }
        public ILogger CreateLogger(string categoryName) => new TestLogger();
    }

    private class TestLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
