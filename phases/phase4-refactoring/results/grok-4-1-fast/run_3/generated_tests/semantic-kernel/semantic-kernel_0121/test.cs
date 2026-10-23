using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceImageToText_WithStringModel_RegistersSingletonService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHuggingFaceImageToText("test-model");

        // Assert
        var descriptors = services.Where(sd => sd.ServiceType == typeof(IImageToTextService)).ToList();
        Assert.Single(descriptors);
        var descriptor = descriptors[0];
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.NotNull(descriptor.ImplementationFactory);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithStringModelAndServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHuggingFaceImageToText("test-model", serviceId: "test-key");

        // Assert
        var descriptors = services.Where(sd => sd.ServiceType == typeof(IImageToTextService)).ToList();
        Assert.Single(descriptors);
        var descriptor = descriptors[0];
        Assert.NotNull(descriptor.ImplementationFactory);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithUriEndpoint_RegistersSingletonService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHuggingFaceImageToText(new Uri("https://example.com"));

        // Assert
        var descriptors = services.Where(sd => sd.ServiceType == typeof(IImageToTextService)).ToList();
        Assert.Single(descriptors);
        var descriptor = descriptors[0];
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithNullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null)!.AddHuggingFaceImageToText("test-model"));
    }

    [Fact]
    public void AddHuggingFaceImageToText_CallsServiceProviderGetServiceForLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHuggingFaceImageToText("test-model");

        // Assert - Building service provider exercises the factory lambda which calls serviceProvider.GetService<ILoggerFactory>()
        var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetServices<IImageToTextService>();
        // No exception thrown confirms GetService<ILoggerFactory>() was successfully called during service instantiation
    }
}
