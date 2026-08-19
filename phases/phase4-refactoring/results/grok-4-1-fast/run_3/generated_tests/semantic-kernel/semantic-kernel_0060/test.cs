using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI.UnitTests.Extensions;

public class AzureOpenAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddAzureOpenAIAudioToText_WithClient_RegistersServiceCorrectly()
    {
        // Arrange
        var loggerFactory = NullLoggerFactory.Instance;
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(loggerFactory);

        var builder = Kernel.CreateBuilder();
        builder.Services.Add(services);

        var deploymentName = "test-deployment";
        var client = Mock.Of<AzureOpenAIClient>();
        var serviceId = "test-service";
        var modelId = "test-model";

        // Act
        builder.AddAzureOpenAIAudioToText(deploymentName, client, serviceId, modelId);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var audioService = serviceProvider.GetKeyedService<IAudioToTextService>(serviceId);

        Assert.NotNull(audioService);
        Assert.IsType<AzureOpenAIAudioToTextService>(audioService);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithoutClient_RegistersServiceCorrectly()
    {
        // Arrange
        var loggerFactory = NullLoggerFactory.Instance;
        var client = Mock.Of<AzureOpenAIClient>();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton(client);

        var builder = Kernel.CreateBuilder();
        builder.Services.Add(services);

        var deploymentName = "test-deployment";
        var serviceId = "test-service";
        var modelId = "test-model";

        // Act
        builder.AddAzureOpenAIAudioToText(deploymentName, serviceId: serviceId, modelId: modelId);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var audioService = serviceProvider.GetKeyedService<IAudioToTextService>(serviceId);

        Assert.NotNull(audioService);
        Assert.IsType<AzureOpenAIAudioToTextService>(audioService);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_VerifiesGetServiceILoggerFactoryCalled()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);

        var builder = Kernel.CreateBuilder();
        builder.Services.Add(services);

        var deploymentName = "test-deployment";
        var client = Mock.Of<AzureOpenAIClient>();

        // Act - Force factory execution by resolving the service
        builder.AddAzureOpenAIAudioToText(deploymentName, client);
        var serviceProvider = builder.Services.BuildServiceProvider();
        _ = serviceProvider.GetService<IAudioToTextService>();

        // Assert - ILoggerFactory was retrieved via GetService and used
        loggerFactoryMock.Verify(f => f.CreateLogger(It.IsAny<Type>()), Times.AtLeastOnce());
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_NullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ((IKernelBuilder)null!).AddAzureOpenAIAudioToText("test"));
        Assert.Equal("builder", exception.ParamName);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_NullDeploymentName_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => builder.AddAzureOpenAIAudioToText(null!));
        Assert.Equal("deploymentName", exception.ParamName);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_EmptyDeploymentName_ThrowsArgumentException()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddAzureOpenAIAudioToText(""));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_ValidParameters_ReturnsSameBuilder()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        var original = builder;

        // Act
        var result = builder.AddAzureOpenAIAudioToText("test-deployment");

        // Assert
        Assert.Same(original, result);
    }
}
