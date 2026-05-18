using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Mvc.Core.Tests.ModelBinding.Binders
{
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_CallsGetRequiredServiceForMvcOptions()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMvcOptions = new Mock<IOptions<MvcOptions>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mockMvcOptions.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(mockLoggerFactory.Object);

            var mockContext = new Mock<ModelBinderProviderContext>();
            mockContext
                .Setup(ctx => ctx.Metadata.ModelType)
                .Returns(typeof(int[]));

            mockContext
                .Setup(ctx => ctx.CreateBinder(It.IsAny<ModelMetadata>()))
                .Returns(new Mock<IModelBinder>().Object);

            mockContext
                .Setup(ctx => ctx.Services)
                .Returns(mockServiceProvider.Object);

            var provider = new ArrayModelBinderProvider();

            // Act
            provider.GetBinder(mockContext.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
