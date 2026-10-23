using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.HuggingFace
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_WithModel_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);

            // Act
            var returnedServices = services.AddHuggingFaceImageToText("test-model", apiKey: "key", serviceId: "myService");

            // Assert
            Assert.Same(services, returnedServices);

            var registration = services.FirstOrDefault(sd => sd.ServiceType == typeof(IImageToTextService));
            Assert.NotNull(registration);
            Assert.NotNull(registration.ImplementationFactory);
        }

        [Fact]
        public void AddHuggingFaceImageToText_WithEndpoint_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);

            var endpoint = new Uri("https://example.com");

            // Act
            var returnedServices = services.AddHuggingFaceImageToText(endpoint, apiKey: "key", serviceId: "myService");

            // Assert
            Assert.Same(services, returnedServices);

            var registration = services.FirstOrDefault(sd => sd.ServiceType == typeof(IImageToTextService));
            Assert.NotNull(registration);
            Assert.NotNull(registration.ImplementationFactory);
        }
    }
}
