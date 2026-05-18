using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Xunit;
using Moq;
using Azure.AI.OpenAI;
using Azure;

namespace Microsoft.SemanticKernel.Tests.Connectors.AzureOpenAI.Extensions
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_UsesProvidedAzureOpenAIClient()
        {
            // Arrange
            var builder = new KernelBuilderMock();
            var deploymentName = "deployment1";
            var modelId = "model1";
            var serviceId = "service1";

            var mockClient = new AzureOpenAIClientMock();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            builder.Services.AddSingleton(mockLoggerFactory.Object);

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, mockClient, serviceId, modelId);

            // Assert
            var service = builder.Services.GetService<IAudioToTextService>(serviceId);
            Assert.NotNull(service);
            Assert.IsType<AzureOpenAIAudioToTextService>(service);

            var azureService = (AzureOpenAIAudioToTextService)service;
            Assert.Equal(deploymentName, azureService.DeploymentName);
            Assert.Equal(modelId, azureService.ModelId);
            Assert.Same(mockClient, azureService.Client);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ResolvesAzureOpenAIClientFromServiceProvider()
        {
            // Arrange
            var builder = new KernelBuilderMock();
            var deploymentName = "deployment2";
            var modelId = "model2";
            var serviceId = "service2";

            var mockClient = new AzureOpenAIClientMock();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            builder.Services.AddSingleton(mockClient);
            builder.Services.AddSingleton(mockLoggerFactory.Object);

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, null, serviceId, modelId);

            // Assert
            var service = builder.Services.GetService<IAudioToTextService>(serviceId);
            Assert.NotNull(service);
            Assert.IsType<AzureOpenAIAudioToTextService>(service);

            var azureService = (AzureOpenAIAudioToTextService)service;
            Assert.Equal(deploymentName, azureService.DeploymentName);
            Assert.Equal(modelId, azureService.ModelId);
            Assert.Same(mockClient, azureService.Client);
        }

        // Mock classes to simulate the environment

        private class KernelBuilderMock : IKernelBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollectionMock();

            // Minimal implementation for IKernelBuilder interface
            public IDictionary<string, object> Plugins => new Dictionary<string, object>();

            public IKernelBuilder AddAzureOpenAIAudioToText(string deploymentName, AzureOpenAIClient? openAIClient = null, string? serviceId = null, string? modelId = null)
            {
                return AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(this, deploymentName, openAIClient, serviceId, modelId);
            }
        }

        private class ServiceCollectionMock : IServiceCollection
        {
            private readonly Dictionary<Type, object> _services = new();
            private readonly Dictionary<string?, object> _keyedServices = new();

            public void AddSingleton<T>(T instance) where T : class
            {
                _services[typeof(T)] = instance!;
            }

            public void AddKeyedSingleton<T>(string? key, Func<IServiceProvider, object?, T> factory) where T : class
            {
                var service = factory(new ServiceProviderMock(_services), null);
                _keyedServices[key] = service!;
            }

            public T? GetService<T>(string? key = null) where T : class
            {
                if (key == null)
                {
                    if (_services.TryGetValue(typeof(T), out var service))
                    {
                        return service as T;
                    }
                }
                else
                {
                    if (_keyedServices.TryGetValue(key, out var service))
                    {
                        return service as T;
                    }
                }
                return null;
            }

            // IServiceCollection members (not used in test)
            public int Count => 0;
            public bool IsReadOnly => false;
            public void Add(ServiceDescriptor item) { }
            public void Clear() { }
            public bool Contains(ServiceDescriptor item) => false;
            public void CopyTo(ServiceDescriptor[] array, int arrayIndex) { }
            public bool Remove(ServiceDescriptor item) => false;
            public System.Collections.Generic.IEnumerator<ServiceDescriptor> GetEnumerator() => System.Linq.Enumerable.Empty<ServiceDescriptor>().GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
            public int IndexOf(ServiceDescriptor item) => -1;
            public void Insert(int index, ServiceDescriptor item) { }
            public void RemoveAt(int index) { }
            public ServiceDescriptor this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }

        private class ServiceProviderMock : IServiceProvider
        {
            private readonly Dictionary<Type, object> _services;

            public ServiceProviderMock(Dictionary<Type, object> services)
            {
                _services = services;
            }

            public object? GetService(Type serviceType)
            {
                _services.TryGetValue(serviceType, out var service);
                return service;
            }
        }

        private class AzureOpenAIClientMock : AzureOpenAIClient
        {
            public AzureOpenAIClientMock() : base(new Uri("http://localhost"), new AzureKeyCredential("fake"), new AzureOpenAIClientOptions())
            {
            }
        }
    }
}
