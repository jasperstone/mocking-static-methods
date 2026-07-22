using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ImageToText;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_WithModel_RegistersServiceAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var model = "test-model";
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

            // Act
            services.AddHuggingFaceImageToText(model);

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<IImageToTextService>();
            Assert.NotNull(service);
            Assert.Equal("Microsoft.SemanticKernel.Connectors.HuggingFace.HuggingFaceImageToTextService", service.GetType().FullName);
        }

        [Fact]
        public void AddHuggingFaceImageToText_WithEndpoint_RegistersServiceAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var endpoint = new Uri("https://example.com");
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

            // Act
            services.AddHuggingFaceImageToText(endpoint);

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<IImageToTextService>();
            Assert.NotNull(service);
            Assert.Equal("Microsoft.SemanticKernel.Connectors.HuggingFace.HuggingFaceImageToTextService", service.GetType().FullName);
        }
    }
}
