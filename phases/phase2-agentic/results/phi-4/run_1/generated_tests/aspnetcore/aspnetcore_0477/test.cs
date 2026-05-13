using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Tests
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WhenModelTypeIsDictionary_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMvcOptions = new Mock<IOptions<MvcOptions>>();
            mockMvcOptions.Setup(m => m.Value).Returns(new MvcOptions());

            mockServiceProvider.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mockMvcOptions.Object);

            var mockContext = new Mock<ModelBinderProviderContext>();
            mockContext.Setup(c => c.Services).Returns(mockServiceProvider.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(mockContext.Object);

            // Assert
            Assert.NotNull(binder);
            mockServiceProvider.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
