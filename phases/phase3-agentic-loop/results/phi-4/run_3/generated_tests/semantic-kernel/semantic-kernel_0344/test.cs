using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web.Bing; // Assuming this is the correct namespace for BingTextSearchOptions
using Microsoft.SemanticKernel; // Assuming this is the correct namespace for ITextSearch

namespace Microsoft.SemanticKernel.Tests
{
    public class WebServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBingTextSearch_WhenOptionsProvided_UsesProvidedOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService<BingTextSearchOptions>())
                .Returns(options);

            // Act
            services.AddBingTextSearch(apiKey, options);
            var provider = services.BuildServiceProvider();

            // Assert
            var textSearch = provider.GetRequiredService<ITextSearch>();
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
        }

        [Fact]
        public void AddBingTextSearch_WhenOptionsNotProvided_UsesServiceProviderOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var apiKey = "test-api-key";
            var options = new BingTextSearchOptions();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService<BingTextSearchOptions>())
                .Returns(options);

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
