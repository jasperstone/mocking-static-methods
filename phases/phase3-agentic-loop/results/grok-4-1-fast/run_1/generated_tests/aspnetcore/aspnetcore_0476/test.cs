using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_CallsGetRequiredService_WhenDictionaryType()
        {
            // Arrange
            var mockServices = new Mock<IServiceProvider>();
            var loggerFactory = NullLoggerFactory.Instance;
            var mvcOptions = new MvcOptions();
            mockServices.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(loggerFactory);
            mockServices.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(Options.Create(mvcOptions));

            var mockMetadataProvider = new Mock<IModelMetadataProvider>();
            mockMetadataProvider.Setup(m => m.GetMetadataForType(typeof(string))).Returns(Mock.Of<ModelMetadata>(mm => mm.ModelType == typeof(string)));
            mockMetadataProvider.Setup(m => m.GetMetadataForType(typeof(int))).Returns(Mock.Of<ModelMetadata>(mm => mm.ModelType == typeof(int)));

            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Services).Returns(mockServices.Object);
            context.Setup(c => c.MetadataProvider).Returns(mockMetadataProvider.Object);
            context.Setup(c => c.Metadata).Returns(Mock.Of<ModelMetadata>(mm => mm.ModelType == typeof(Dictionary<string, int>)));
            context.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(Mock.Of<IModelBinder>());

            // Act
            var provider = new DictionaryModelBinderProvider();
            var result = provider.GetBinder(context.Object);

            // Assert
            mockServices.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once());
            mockServices.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once());
            Assert.NotNull(result);
        }

        [Fact]
        public void GetBinder_DoesNotCallGetRequiredService_WhenNotDictionaryType()
        {
            // Arrange
            var mockServices = new Mock<IServiceProvider>();

            var mockMetadataProvider = new Mock<IModelMetadataProvider>();
            mockMetadataProvider.Setup(m => m.GetMetadataForType(typeof(string))).Returns(Mock.Of<ModelMetadata>(mm => mm.ModelType == typeof(string)));

            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Services).Returns(mockServices.Object);
            context.Setup(c => c.MetadataProvider).Returns(mockMetadataProvider.Object);
            context.Setup(c => c.Metadata).Returns(Mock.Of<ModelMetadata>(mm => mm.ModelType == typeof(string)));

            // Act
            var provider = new DictionaryModelBinderProvider();
            var result = provider.GetBinder(context.Object);

            // Assert
            mockServices.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Never());
            mockServices.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Never());
            Assert.Null(result);
        }
    }
}
