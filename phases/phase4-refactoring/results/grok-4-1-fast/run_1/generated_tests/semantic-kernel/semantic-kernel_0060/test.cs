using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI.Tests.Extensions;

public class AzureOpenAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddAzureOpenAIAudioToText_WithClient_RegistersKeyedSingletonWithFactoryUsingGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);

        var builder = Kernel.CreateBuilder();
        builder.Services.Add(services);

        var client = new Mock<AzureOpenAIClient>().Object;
        const string deploymentName = "test-deployment";
        const string serviceId = "test-service";

        // Act
        builder.AddAzureOpenAIAudioToText(deploymentName, client, serviceId, null);

        // Assert registration happened
        Assert.Contains(services, d => d.ServiceType == typeof(IAudioToTextService) && d.Lifetime == ServiceLifetime.Singleton);

        // Build and resolve to trigger factory with GetService<ILoggerFactory>()
        var serviceProvider = services.BuildServiceProvider();
        var audioServices = serviceProvider.GetKeyedServices<IAudioToTextService>(serviceId);
        var audioService = Assert.Single(audioServices);
        Assert.NotNull(audioService);

        // Verify logger factory was used via GetService
        mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithoutClient_UsesGetRequiredServiceForClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var client = new Mock<AzureOpenAIClient>().Object;
        services.AddSingleton(client);

        var builder = Kernel.CreateBuilder();
        builder.Services.Add(services);

        const string deploymentName = "test-deployment";
        const string serviceId = "test-service";

        // Act
        builder.AddAzureOpenAIAudioToText(deploymentName, serviceId: serviceId);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var audioServices = serviceProvider.GetKeyedServices<IAudioToTextService>(serviceId);
        Assert.NotNull(Assert.Single(audioServices));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_NoLoggerFactory_HandlesNullFromGetService()
    {
        // Arrange
        var services = new ServiceCollection();

        var builder = Kernel.CreateBuilder();
        builder.Services.Add(services);

        var client = new Mock<AzureOpenAIClient>().Object;
        const string deploymentName = "test-deployment";
        const string serviceId = "test-service";

        // Act
        builder.AddAzureOpenAIAudioToText(deploymentName, client, serviceId);

        // Assert - GetService<ILoggerFactory>() returns null but service still created
        var serviceProvider = services.BuildServiceProvider();
        var audioServices = serviceProvider.GetKeyedServices<IAudioToTextService>(serviceId);
        Assert.NotNull(Assert.Single(audioServices));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_ValidatesParameters()
    {
        var builder = Kernel.CreateBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.AddAzureOpenAIAudioToText(null!));
        Assert.Throws<ArgumentException>(() => builder.AddAzureOpenAIAudioToText(""));
        Assert.Throws<ArgumentException>(() => builder.AddAzureOpenAIAudioToText(" \t"));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_ReturnsSameBuilderInstance()
    {
        var builder = Kernel.CreateBuilder();
        var result = builder.AddAzureOpenAIAudioToText("test-deployment");
        Assert.Same(builder, result);
    }
}
