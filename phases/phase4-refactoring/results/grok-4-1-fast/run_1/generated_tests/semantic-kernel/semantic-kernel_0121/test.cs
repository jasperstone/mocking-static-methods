using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.ImageToText;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.UnitTests;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceImageToText_WithModel_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory);

        string model = "test-model";
        string? serviceId = "test-service-id";

        // Act
        var result = services.AddHuggingFaceImageToText(model, serviceId: serviceId);

        // Assert
        Assert.Equal(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(serviceId);
        Assert.NotNull(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithEndpoint_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory);

        Uri endpoint = new("https://test-endpoint.com");
        string? serviceId = "test-service-id";

        // Act
        var result = services.AddHuggingFaceImageToText(endpoint, serviceId: serviceId);

        // Assert
        Assert.Equal(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(serviceId);
        Assert.NotNull(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        string model = "test-model";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddHuggingFaceImageToText(model));
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithNullServiceId_RegistersDefaultService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddHuggingFaceImageToText("test-model");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetService<IImageToTextService>();
        Assert.NotNull(imageService);
    }

    private class MockLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider) { }
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => NullLogger.Instance;
        public void Dispose() { }
    }
}
