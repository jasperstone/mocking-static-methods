using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web;

namespace WebServiceCollectionExtensionsTests
{
    public class AddBingTextSearchTests
    {
        [Fact]
        public void AddsBingTextSearch_WithNullOptions_CallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register a dummy service provider
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<BingTextSearchOptions>())
                .Returns(new BingTextSearchOptions());

            // Register the service provider in the service collection
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            var result = WebServiceCollectionExtensions.AddBingTextSearch(
                services,
                "testApiKey",
                null,
                "testServiceId");

            // Build the provider
            var provider = services.BuildServiceProvider();

            // Retrieve the registered factory delegate for ITextSearch
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITextSearch));
            Assert.NotNull(descriptor);

            // The implementation should be a Func<IServiceProvider, object>
            var factory = descriptor.ImplementationInstance as Func<IServiceProvider, object>;
            Assert.NotNull(factory);

            // Act: invoke the factory to create the ITextSearch instance
            var textSearch = factory(provider) as ITextSearch;
            Assert.NotNull(textSearch);
            Assert.IsType<BingTextSearch>(textSearch);

            // Verify that GetService<BingTextSearchOptions>() was called
            serviceProviderMock.Verify(sp => sp.GetService<BingTextSearchOptions>(), Times.Once);
        }
    }
}
