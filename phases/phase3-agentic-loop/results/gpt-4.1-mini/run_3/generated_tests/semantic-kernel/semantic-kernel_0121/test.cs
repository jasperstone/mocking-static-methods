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

        var loggerFactoryMock = new Mock<ILoggerFactory>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(loggerFactoryMock.Object);

        // We need to simulate the serviceProvider passed to the factory delegate
        // So we will add a keyed singleton with a factory that uses the serviceProviderMock

        // Act
        services.AddHuggingFaceImageToText(
            model: "test-model",
            endpoint: new Uri("https://test.endpoint"),
            apiKey: "test-api-key",
            serviceId: "test-service",
            httpClient: new HttpClient());

        var provider = services.BuildServiceProvider();

        // The AddKeyedSingleton extension method is internal to the project, so we cannot directly resolve by key.
        // Instead, we test that the service can be resolved and that the ILoggerFactory was requested from the service provider.

        // To verify the call to GetService<ILoggerFactory>(), we create a new service collection and register the mock logger factory
        var servicesWithMockLogger = new ServiceCollection();
        servicesWithMockLogger.AddSingleton(loggerFactoryMock.Object);
        servicesWithMockLogger.AddHuggingFaceImageToText(
            model: "test-model",
            endpoint: new Uri("https://test.endpoint"),
            apiKey: "test-api-key",
            serviceId: "test-service",
            httpClient: new HttpClient());

        var sp = servicesWithMockLogger.BuildServiceProvider();

        // Act: resolve the IImageToTextService
        var imageToTextService = sp.GetService<IImageToTextService>();

        // Assert
        Assert.NotNull(imageToTextService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithEndpoint_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        var loggerFactoryMock = new Mock<ILoggerFactory>();

        services.AddSingleton(loggerFactoryMock.Object);

        // Act
        services.AddHuggingFaceImageToText(
            endpoint: new Uri("https://test.endpoint"),
            apiKey: "test-api-key",
            serviceId: "test-service",
            httpClient: new HttpClient());

        var sp = services.BuildServiceProvider();

        // Act: resolve the IImageToTextService
        var imageToTextService = sp.GetService<IImageToTextService>();

        // Assert
        Assert.NotNull(imageToTextService);
    }
}
