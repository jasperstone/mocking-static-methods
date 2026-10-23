using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Microsoft.SemanticKernel.Plugins.Web;

namespace WebServiceCollectionExtensionsTests
{
    public class AddBingTextSearchTests
    {
        [Fact]
        public void AddsBingTextSearch_WithOptionsNull_CallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new BingTextSearchOptions();

            // Mock IServiceProvider to return options when GetService is called
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<BingTextSearchOptions>())
                .Returns(options);

            // Register the mock IServiceProvider in the service collection
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddBingTextSearch("apiKey", options: null);

            // Build the provider
            var provider = services.BuildServiceProvider();

            // Retrieve the ITextSearch instance
            var textSearch = provider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);
            var bingTextSearch = (BingTextSearch)textSearch;
            Assert.Equal("apiKey", bingTextSearch.ApiKey);
        }
    }
}
