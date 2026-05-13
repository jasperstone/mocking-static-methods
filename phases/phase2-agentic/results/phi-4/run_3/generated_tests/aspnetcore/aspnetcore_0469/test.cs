using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WhenModelTypeIsArray_CallsGetRequiredServiceForIOptionsMvcOptions()
        {
            // Arrange
            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Metadata.ModelType).Returns(typeof(int[]));
            context.Setup(c => c.Metadata.ElementMetadata).Returns(new Mock<ModelMetadata>().Object);
            context.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(new Mock<IModelBinder>().Object);

            var serviceProvider = new Mock<IServiceProvider>();
            var mvcOptions = new MvcOptions();
            var options = Options.Create(mvcOptions);

            serviceProvider.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(options);

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context.Object);

            // Assert
            serviceProvider.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
            Assert.NotNull(binder);
        }
    }
}
