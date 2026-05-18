using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.ImageToText;
using Microsoft.SemanticKernel.TextGeneration;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextGeneration_WithModel_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextGeneration(
                services,
                "test-model",
                new Uri("http://localhost"),
                "api-key",
                "serviceId",
                new HttpClient());

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddHuggingFaceTextGeneration_WithEndpoint_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextGeneration(
                services,
                new Uri("http://localhost"),
                "api-key",
                "serviceId",
                new HttpClient());

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_WithModel_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceChatCompletion(
                services,
                "test-model",
                new Uri("http://localhost"),
                "api-key",
                "serviceId",
                new HttpClient());

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_WithEndpoint_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceChatCompletion(
                services,
                new Uri("http://localhost"),
                "api-key",
                "serviceId",
                new HttpClient());

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_WithModel_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(
                services,
                "test-model",
                new Uri("http://localhost"),
                "api-key",
                "serviceId",
                new HttpClient());

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_WithEndpoint_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceTextEmbeddingGeneration(
                services,
                new Uri("http://localhost"),
                "api-key",
                "serviceId",
                new HttpClient());

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddHuggingFaceImageToText_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(
                services,
                "test-model",
                new Uri("http://localhost"),
                "api-key",
                "serviceId",
                new HttpClient());

            // Assert
            Assert.Same(services, result);
        }
    }
}
