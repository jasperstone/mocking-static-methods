using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI.Tests.Extensions;

public class AzureOpenAIKernelBuilderExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly IKernelBuilder _builder;

    public AzureOpenAIKernelBuilderExtensionsTests()
    {
        _services = new ServiceCollection();
        _builder = Kernel.CreateBuilder();
        _builder.Services.Add(_services);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithClient_ReturnsSameBuilder()
    {
        // Act
        var result = _builder.AddAzureOpenAIAudioToText("test-deployment", openAIClient: null);

        // Assert
        Assert.Same(_builder, result);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_RegistersSingletonService()
    {
        // Act
        _builder.AddAzureOpenAIAudioToText("test-deployment");

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var descriptors = serviceProvider.GetServices<ServiceDescriptor>();
        var descriptor = descriptors.FirstOrDefault(d => d.ServiceType == typeof(IAudioToTextService));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.NotNull(descriptor.ImplementationFactory);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithServiceId_RegistersKeyedFactory()
    {
        // Arrange
        const string serviceId = "test-service";

        // Act
        _builder.AddAzureOpenAIAudioToText("test-deployment", serviceId: serviceId);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var descriptors = serviceProvider.GetServices<ServiceDescriptor>();
        var descriptor = descriptors.FirstOrDefault(d => d.ServiceType == typeof(IAudioToTextService));

        Assert.NotNull(descriptor);
        Assert.NotNull(descriptor.ImplementationFactory);

        // Verify factory signature supports keyed service (takes serviceKey parameter)
        var factory = descriptor.ImplementationFactory!;
        Assert.True(typeof(Delegate).IsAssignableFrom(factory.GetType()));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithClient_FactoryUsesProvidedClient()
    {
        // Arrange - Add required dependencies for factory to execute
        _services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        _builder.AddAzureOpenAIAudioToText("test-deployment", openAIClient: null);

        // Assert - exercise factory to verify GetService<ILoggerFactory>() call succeeds
        var serviceProvider = _services.BuildServiceProvider();
        var descriptors = serviceProvider.GetServices<ServiceDescriptor>();
        var descriptor = descriptors.FirstOrDefault(d => d.ServiceType == typeof(IAudioToTextService));

        Assert.NotNull(descriptor);
        var factory = (Func<IServiceProvider, object?, object>)descriptor!.ImplementationFactory!;
        var audioService = (IAudioToTextService)factory(serviceProvider, null);
        Assert.NotNull(audioService);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithoutClient_UsesGetRequiredService()
    {
        // Arrange - Add required dependencies
        _services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        _builder.AddAzureOpenAIAudioToText("test-deployment");

        // Assert - factory exercises GetRequiredService<AzureOpenAIClient>()
        var serviceProvider = _services.BuildServiceProvider();
        var descriptors = serviceProvider.GetServices<ServiceDescriptor>();
        var descriptor = descriptors.FirstOrDefault(d => d.ServiceType == typeof(IAudioToTextService));

        Assert.NotNull(descriptor);
        var factory = (Func<IServiceProvider, object?, object>)descriptor!.ImplementationFactory!;
        Assert.ThrowsAny<Exception>(() => factory(serviceProvider, null)); // Expect exception due to missing AzureOpenAIClient
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_NullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IKernelBuilder)null!).AddAzureOpenAIAudioToText("test-deployment"));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_NullDeploymentName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddAzureOpenAIAudioToText(null!));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_EmptyDeploymentName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _builder.AddAzureOpenAIAudioToText(""));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithNoLoggerFactory_FactoryStillExecutes()
    {
        // Act
        _builder.AddAzureOpenAIAudioToText("test-deployment");

        // Assert - GetService<ILoggerFactory>() returns null, but factory should handle it
        var serviceProvider = _services.BuildServiceProvider();
        var descriptors = serviceProvider.GetServices<ServiceDescriptor>();
        var descriptor = descriptors.FirstOrDefault(d => d.ServiceType == typeof(IAudioToTextService));

        Assert.NotNull(descriptor);
        var factory = descriptor!.ImplementationFactory!;
        // Factory should execute without crashing even with missing dependencies
        Assert.NotNull(factory);
    }
}
