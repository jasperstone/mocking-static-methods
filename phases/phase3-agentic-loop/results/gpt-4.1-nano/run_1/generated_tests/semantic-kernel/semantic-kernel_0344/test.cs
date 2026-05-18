using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web;

namespace WebServiceCollectionExtensionsTests
{
    public class AddTextSearchExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_ShouldRegisterService_WithDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";

            // Act
            services.AddBingTextSearch(apiKey);
            var provider = services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<ITextSearch>();
            Assert.NotNull(service);
            Assert.IsType<BingTextSearch>(service);
        }

        [Fact]
        public void AddBraveTextSearch_ShouldRegisterService_WithCustomOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "brave-api-key";
            var options = new BraveTextSearchOptions();

            // Act
            services.AddBraveTextSearch(apiKey, options);
            var provider = services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<ITextSearch>();
            Assert.NotNull(service);
            Assert.IsType<BraveTextSearch>(service);
        }

        [Fact]
        public void AddGoogleTextSearch_ShouldRegisterService_WithCustomOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var searchEngineId = "search-engine-id";
            var apiKey = "google-api-key";
            var options = new GoogleTextSearchOptions();

            // Act
            services.AddGoogleTextSearch(searchEngineId, apiKey, options);
            var provider = services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<ITextSearch>();
            Assert.NotNull(service);
            Assert.IsType<GoogleTextSearch>(service);
        }

        [Fact]
        public void AddTavilyTextSearch_ShouldRegisterService_WithCustomOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "tavily-api-key";

            // Act
            services.AddTavilyTextSearch(apiKey);
            var provider = services.BuildServiceProvider();

            // Assert
            var service = provider.GetService<ITextSearch>();
            Assert.NotNull(service);
            Assert.IsType<TavilyTextSearch>(service);
        }

        [Fact]
        public void AddBingTextSearch_ShouldUseServiceProviderToGetOptions_WhenOptionsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";

            var optionsInstance = new BingTextSearchOptions();
            services.AddSingleton(optionsInstance);

            // Register the extension
            services.AddBingTextSearch(apiKey);

            var provider = services.BuildServiceProvider();

            // Act
            var service = provider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<BingTextSearch>(service);
        }
    }
}
