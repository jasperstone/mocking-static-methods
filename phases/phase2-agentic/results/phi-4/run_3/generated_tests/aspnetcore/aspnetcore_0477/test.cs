using System;
using Microsoft.AspNetCore.Mvc;
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
        public void GetBinder_ShouldRetrieveMvcOptionsFromServices()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMvcOptions = new Mock<IOptions<MvcOptions>>();
            var mvcOptions = new MvcOptions();
            mockMvcOptions.Setup(o => o.Value).Returns(mvcOptions);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mockMvcOptions.Object);

            var mockMetadataProvider = new Mock<IModelMetadataProvider>();
            var modelType = typeof(IDictionary<string, int>);
            var metadata = new Mock<ModelMetadata>();
            mockMetadataProvider.Setup(m => m.GetMetadataForType(modelType)).Returns(metadata.Object);

            var context = new ModelBinderProviderContext(
                new DictionaryModelBinderProvider(),
                metadata.Object,
                mockServiceProvider.Object);

            // Act
            var binder = ((DictionaryModelBinderProvider)context.Provider).GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            mockServiceProvider.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
