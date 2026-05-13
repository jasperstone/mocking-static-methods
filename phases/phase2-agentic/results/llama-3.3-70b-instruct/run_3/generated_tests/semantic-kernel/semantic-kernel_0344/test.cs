using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Moq;
using Xunit;

namespace SemanticKernelTests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_WithNullOptions_GetServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var bingTextSearchOptions = new BingTextSearchOptions();

            services.AddSingleton(bingTextSearchOptions);

            // Act
            services.AddBingTextSearch(apiKey, serviceId: serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddBraveTextSearch_WithNullOptions_GetServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var braveTextSearchOptions = new BraveTextSearchOptions();

            services.AddSingleton(braveTextSearchOptions);

            // Act
            services.AddBraveTextSearch(apiKey, serviceId: serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddGoogleTextSearch_WithNullOptions_GetServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var searchEngineId = "searchEngineId";
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var googleTextSearchOptions = new GoogleTextSearchOptions();

            services.AddSingleton(googleTextSearchOptions);

            // Act
            services.AddGoogleTextSearch(searchEngineId, apiKey, serviceId: serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddTavilyTextSearch_WithNullOptions_GetServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var tavilyTextSearchOptions = new TavilyTextSearchOptions();

            services.AddSingleton(tavilyTextSearchOptions);

            // Act
            services.AddTavilyTextSearch(apiKey, serviceId: serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>(serviceId);
            Assert.NotNull(textSearch);
        }
    }
}
