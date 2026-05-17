using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel; // Ensure this namespace is included for ITextSearch

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_WhenOptionsProvided_UsesProvidedOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "testApiKey";
            var options = new BingTextSearchOptions { Endpoint = new Uri("https://example.com") };
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<BingTextSearchOptions>()).Returns(options);

            // Act
            services.AddBingTextSearch(apiKey, options);
            var provider = services.BuildServiceProvider();

            // Assert
            var textSearch = provider.GetRequiredService<ITextSearch>();
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_WhenOptionsNotProvided_UsesServiceProvidedOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "testApiKey";
            var options = new BingTextSearchOptions { Endpoint = new Uri("https://example.com") };
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<BingTextSearchOptions>()).Returns(options);

            // Act
            services.AddBingTextSearch(apiKey);
            var provider = services.BuildServiceProvider();

            // Assert
            var textSearch = provider.GetRequiredService<ITextSearch>();
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }
    }
}
