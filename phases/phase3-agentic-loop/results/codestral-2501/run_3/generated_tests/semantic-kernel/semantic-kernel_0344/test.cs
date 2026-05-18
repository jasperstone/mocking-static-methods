using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_ShouldRegisterBingTextSearchWithDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BingTextSearchOptions)))
                .Returns(new BingTextSearchOptions());

            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddBingTextSearch(apiKey);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_ShouldRegisterBingTextSearchWithProvidedOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions
            {
                Endpoint = new Uri("https://test-endpoint.com")
            };

            // Act
            services.AddBingTextSearch(apiKey, options);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }
    }
}
