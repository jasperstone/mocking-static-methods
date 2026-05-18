using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.AzureOpenAI.Extensions
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        private class DummyAzureOpenAIClient : AzureOpenAIClient
        {
            public DummyAzureOpenAIClient() : base(new Uri("http://localhost"), new Azure.Core.ApiKeyCredential("dummy"), new Azure.AI.OpenAI.AzureOpenAIClientOptions())
            {
            }
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_UsesProvidedAzureOpenAIClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new KernelBuilderStub(services);

            var dummyClient = new DummyAzureOpenAIClient();
            var deploymentName = "deployment";
            var modelId = "modelId";
            var serviceId = "serviceId";

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, dummyClient, serviceId, modelId);

            // Build service provider from services to simulate DI container
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the registered IAudioToTextService factory delegate
            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IAudioToTextService));
            Assert.NotNull(descriptor);

            var factory = descriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Invoke the factory with the service provider
            var audioToTextService = factory(serviceProvider);

            // Assert
            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ResolvesAzureOpenAIClientFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new KernelBuilderStub(services);

            var dummyClient = new DummyAzureOpenAIClient();
            var loggerFactory = LoggerFactory.Create(builder => { });

            services.AddSingleton<AzureOpenAIClient>(dummyClient);
            services.AddSingleton<ILoggerFactory>(loggerFactory);

            var deploymentName = "deployment";
            var modelId = "modelId";
            var serviceId = "serviceId";

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, null, serviceId, modelId);

            // Build service provider from services to simulate DI container
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the registered IAudioToTextService factory delegate
            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IAudioToTextService));
            Assert.NotNull(descriptor);

            var factory = descriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Invoke the factory with the service provider
            var audioToTextService = factory(serviceProvider);

            // Assert
            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
        }

        private class KernelBuilderStub : IKernelBuilder
        {
            public IServiceCollection Services { get; }

            public KernelBuilderStub(IServiceCollection services)
            {
                Services = services;
            }
        }
    }
}
