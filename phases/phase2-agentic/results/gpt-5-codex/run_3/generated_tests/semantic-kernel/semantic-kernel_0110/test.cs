using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.UnitTests.Extensions;

public class VertexAIServiceCollectionExtensionsTests
{
    private const string ModelId = "test-model";
    private const string BearerKey = "test-key";
    private const string Location = "test-location";
    private const string ProjectId = "test-project";

    [Fact]
    public void AddVertexAIEmbeddingGenerator_UsesLoggerFactoryFromServiceProvider()
    {
        // Arrange
        const string serviceId = "service-with-logger";
        var services = new ServiceCollection();
        var trackingLoggerFactory = new TrackingLoggerFactory();
        services.AddSingleton<ILoggerFactory>(trackingLoggerFactory);

        services.AddVertexAIEmbeddingGenerator(
            modelId: ModelId,
            bearerKey: BearerKey,
            location: Location,
            projectId: ProjectId,
            apiVersion: VertexAIVersion.V1,
            serviceId: serviceId,
            httpClient: new HttpClient());

        using var provider = services.BuildServiceProvider();

        // Act
        var generator = provider.GetRequiredKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(serviceId);

        // Assert
        Assert.NotNull(generator);
        Assert.True(trackingLoggerFactory.CreateLoggerCalled);
        Assert.Equal(generator.GetType().FullName, trackingLoggerFactory.CategoryName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_AllowsMissingLoggerFactory()
    {
        // Arrange
        const string serviceId = "service-without-logger";
        var services = new ServiceCollection();

        services.AddVertexAIEmbeddingGenerator(
            modelId: ModelId,
            bearerKey: BearerKey,
            location: Location,
            projectId: ProjectId,
            apiVersion: VertexAIVersion.V1,
            serviceId: serviceId,
            httpClient: new HttpClient());

        using var provider = services.BuildServiceProvider();

        // Act
        var generator = provider.GetRequiredKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(serviceId);

        // Assert
        Assert.NotNull(generator);

        var loggerField = generator.GetType().GetField("_logger", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(loggerField);
        Assert.Null(loggerField!.GetValue(generator));
    }

    private sealed class TrackingLoggerFactory : ILoggerFactory
    {
        public bool CreateLoggerCalled { get; private set; }
        public string? CategoryName { get; private set; }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            this.CreateLoggerCalled = true;
            this.CategoryName = categoryName;
            return new TrackingLogger();
        }

        public void Dispose()
        {
        }

        private sealed class TrackingLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => false;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();

            public void Dispose()
            {
            }
        }
    }
}
