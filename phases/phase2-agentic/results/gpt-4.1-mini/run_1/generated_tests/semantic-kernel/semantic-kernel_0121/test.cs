using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.HuggingFace;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceImageToText_WithModel_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var model = "test-model";
        var endpoint = new Uri("https://test-endpoint");
        var apiKey = "test-api-key";
        var serviceId = "test-service-id";
        var httpClient = new HttpClient();

        var loggerFactoryMock = new Mock<ILoggerFactory>();

        // We need to mock the IServiceProvider passed to the factory delegate
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(loggerFactoryMock.Object);

        // Add a dummy AddKeyedSingleton extension method to IServiceCollection for testing
        // Since the real AddKeyedSingleton is not accessible here, we simulate the call
        services.AddKeyedSingleton = (string id, Func<IServiceProvider, object, object> factory) =>
        {
            // Call the factory with the mocked service provider and null for the second argument
            var service = factory(serviceProviderMock.Object, null);
            Assert.NotNull(service);
            return services;
        };

        // Act
        var returnedServices = HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(
            services,
            model,
            endpoint,
            apiKey,
            serviceId,
            httpClient);

        // Assert
        Assert.Same(services, returnedServices);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithEndpointOnly_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var endpoint = new Uri("https://test-endpoint");
        var apiKey = "test-api-key";
        var serviceId = "test-service-id";
        var httpClient = new HttpClient();

        var loggerFactoryMock = new Mock<ILoggerFactory>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(loggerFactoryMock.Object);

        services.AddKeyedSingleton = (string id, Func<IServiceProvider, object, object> factory) =>
        {
            var service = factory(serviceProviderMock.Object, null);
            Assert.NotNull(service);
            return services;
        };

        // Act
        var returnedServices = HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(
            services,
            endpoint,
            apiKey,
            serviceId,
            httpClient);

        // Assert
        Assert.Same(services, returnedServices);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
    }
}

// Extension method to simulate AddKeyedSingleton for testing purposes
public static class ServiceCollectionExtensionsForTest
{
    public static IServiceCollection AddKeyedSingleton<TService>(
        this IServiceCollection services,
        string? serviceId,
        Func<IServiceProvider, object, TService> factory)
        where TService : class
    {
        // This is a stub for testing, not actual implementation
        return services;
    }
}
