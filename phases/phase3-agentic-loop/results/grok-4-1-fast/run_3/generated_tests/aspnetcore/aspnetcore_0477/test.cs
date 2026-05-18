using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_DictionaryType_ReturnsBinder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IOptions<MvcOptions>>(new OptionsWrapper<MvcOptions>(new MvcOptions()));
            var serviceProvider = services.BuildServiceProvider();

            var metadataProviderMock = new Mock<IModelMetadataProvider>(MockBehavior.Strict);
            var contextMock = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);
            contextMock.Setup(c => c.Services).Returns(serviceProvider);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);
            contextMock.Setup(c => c.Metadata).Returns(CreateModelMetadata(metadataProviderMock.Object, typeof(Dictionary<string, int>)));

            var provider = new DictionaryModelBinderProvider();

            // Act
            var result = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetBinder_NonDictionaryType_ReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IOptions<MvcOptions>>(new OptionsWrapper<MvcOptions>(new MvcOptions()));
            var serviceProvider = services.BuildServiceProvider();

            var metadataProviderMock = new Mock<IModelMetadataProvider>(MockBehavior.Strict);
            var contextMock = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);
            contextMock.Setup(c => c.Services).Returns(serviceProvider);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);
            contextMock.Setup(c => c.Metadata).Returns(CreateModelMetadata(metadataProviderMock.Object, typeof(string)));

            var provider = new DictionaryModelBinderProvider();

            // Act
            var result = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_CallsGetRequiredServiceIOptionsMvcOptions()
        {
            // Arrange
            var mvcOptionsMock = new Mock<MvcOptions>();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            optionsMock.Setup(o => o.Value).Returns(mvcOptionsMock.Object);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(optionsMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var metadataProviderMock = new Mock<IModelMetadataProvider>(MockBehavior.Strict);
            metadataProviderMock.Setup(m => m.GetMetadataForType(typeof(string))).Returns(CreateModelMetadata(metadataProviderMock.Object, typeof(string)));
            metadataProviderMock.Setup(m => m.GetMetadataForType(typeof(int))).Returns(CreateModelMetadata(metadataProviderMock.Object, typeof(int)));

            var contextMock = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);
            contextMock.Setup(c => c.Services).Returns(serviceProvider);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);
            contextMock.Setup(c => c.Metadata).Returns(CreateModelMetadata(metadataProviderMock.Object, typeof(Dictionary<string, int>)));
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(new Mock<IModelBinder>().Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            _ = provider.GetBinder(contextMock.Object);

            // Assert
            optionsMock.Verify(o => o.Value, Times.Once());
        }

        [Fact]
        public void GetBinder_ThrowsIfContextNull()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
            Assert.Equal("context", exception.ParamName);
        }

        private static ModelMetadata CreateModelMetadata(IModelMetadataProvider provider, Type modelType)
        {
            var detailsProviderMock = new Mock<ICompositeMetadataDetailsProvider>(MockBehavior.Strict);
            return new ModelMetadata(provider, detailsProviderMock.Object, modelType, null!, "Test", false, false, false, null!, null!);
        }
    }
}
