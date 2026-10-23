using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web;
using Moq;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_ShouldRegisterBingTextSearch()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();

            // Act
            serviceCollection.AddBingTextSearch(apiKey, options);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_ShouldUseServiceProviderOptions_WhenOptionsNotProvided()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();
            serviceCollection.AddSingleton(options);

            // Act
            serviceCollection.AddBingTextSearch(apiKey);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_ShouldUseProvidedOptions_WhenOptionsProvided()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();

            // Act
            serviceCollection.AddBingTextSearch(apiKey, options);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_ShouldUseServiceId_WhenServiceIdProvided()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();
            var serviceId = "test-service-id";

            // Act
            serviceCollection.AddBingTextSearch(apiKey, options, serviceId);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var textSearch = serviceProvider.GetKeyedService<ITextSearch>(serviceId);

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }
    }
}
