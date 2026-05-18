using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_RegistersBingTextSearch()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();

            // Act
            serviceCollection.AddBingTextSearch(apiKey, options);

            // Assert
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BingTextSearchOptions))).Returns(options);
            var serviceProvider = serviceProviderMock.Object;
            var textSearch = serviceProvider.GetService<ITextSearch>();

            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_UsesDefaultOptions_WhenOptionsAreNull()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";

            // Act
            serviceCollection.AddBingTextSearch(apiKey);

            // Assert
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BingTextSearchOptions))).Returns(new BingTextSearchOptions());
            var serviceProvider = serviceProviderMock.Object;
            var textSearch = serviceProvider.GetService<ITextSearch>();

            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_UsesProvidedOptions_WhenOptionsAreNotNull()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions
            {
                Endpoint = new Uri("https://test-endpoint.com")
            };

            // Act
            serviceCollection.AddBingTextSearch(apiKey, options);

            // Assert
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BingTextSearchOptions))).Returns(options);
            var serviceProvider = serviceProviderMock.Object;
            var textSearch = serviceProvider.GetService<ITextSearch>();

            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_UsesServiceId_WhenServiceIdIsProvided()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();
            var serviceId = "test-service-id";

            // Act
            serviceCollection.AddBingTextSearch(apiKey, options, serviceId);

            // Assert
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(BingTextSearchOptions))).Returns(options);
            var serviceProvider = serviceProviderMock.Object;
            var textSearch = serviceProvider.GetKeyedService<ITextSearch>(serviceId);

            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }
    }
}
